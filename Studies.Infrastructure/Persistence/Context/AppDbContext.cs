using Microsoft.EntityFrameworkCore;
using Studies.Domain.Entities;
using Studies.Domain.Shared;

namespace Studies.Infrastructure.Persistence.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas as IEntityTypeConfiguration<T> encontradas neste assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Filtro global de soft delete — qualquer query já filtra IsDeleted = false automaticamente
        // Isso elimina a necessidade de .Where(x => !x.IsDeleted) em todo lugar
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
    }

    // Intercepta SaveChanges para preencher campos de auditoria automaticamente
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Property(nameof(EntityBase.CreatedAt)).IsModified = false;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
