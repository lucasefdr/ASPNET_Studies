using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Studies.Domain.Interfaces;
using Studies.Infrastructure.Persistence.Context;
using Studies.Infrastructure.Persistence.Repositories;

namespace Studies.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext _context = context;
    private IDbContextTransaction? _currentTransaction;

    private IProductRepository? _products;

    // Lazy initialization
    public IProductRepository Products => _products ??= new ProductRepository(_context);

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Rollback()
    {
        foreach (var entry in _context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = EntityState.Unchanged;
                    break;
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Unchanged;
                    break;
            }
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            await operation();
            return;
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await operation();
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync();
        }
        catch
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }


    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            return await operation();

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation();
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_currentTransaction is not null)
            await _currentTransaction.DisposeAsync();
        await _context.DisposeAsync();
    }
}
