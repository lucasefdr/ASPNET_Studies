using Microsoft.EntityFrameworkCore;
using Studies.Domain.Interfaces;
using Studies.Domain.Shared;
using Studies.Infrastructure.Persistence.Context;
using System.Linq.Expressions;

namespace Studies.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação genérica do contrato de repositório utilizando o Entity Framework Core.
/// </summary>
/// <remarks>
/// <strong>Por que usar um repositório se o DbContext já é uma abstração?</strong><br/>
/// O repositório centraliza a lógica de queries, facilita testes unitários (permitindo o mock de <see cref="IRepository{T}"/> 
/// em vez do DbContext) e isola a camada de domínio de detalhes técnicos de persistência, como <c>Include()</c> e <c>AsNoTracking()</c>.
/// </remarks>
/// <typeparam name="T">O tipo da entidade manipulada pelo repositório. Deve herdar de <see cref="BaseEntity"/>.</typeparam>
public class Repository<T>(AppDbContext context) : IRepository<T> where T : EntityBase, IAggregateRoot
{
    /// <summary>
    /// Conjunto de dados tipado do EF Core. O EF identifica automaticamente qual tabela mapear com base no tipo <typeparamref name="T"/>.
    /// </summary>
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    // ──────────────────────────────────────────────
    // Leituras
    // ──────────────────────────────────────────────

    public virtual IQueryable<T> Get()
    {
        return _dbSet.AsNoTracking();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Utiliza <c>FindAsync</c> nativo. Não aplica <c>AsNoTracking()</c> de forma intencional: se o Unit of Work precisar 
    /// rastrear mudanças futuras nesta entidade para um Update, o EF precisa detectá-las. Para leituras simples, prefira métodos que retornam listas.
    /// </remarks>
    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>
    /// O <c>QueryFilter</c> global de <c>IsDeleted</c> já está aplicado no DbContext.
    /// Utiliza <c>AsNoTracking()</c> para evitar o overhead de rastreamento de mudanças, otimizando o desempenho para operações exclusivas de leitura.
    /// </remarks>
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<T?> SingleOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().SingleOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>
    /// <c>AnyAsync</c> é a forma mais eficiente para esta operação, pois apenas verifica a existência gerando uma instrução equivalente a "SELECT 1 WHERE ...", sem transitar dados desnecessários.
    /// </remarks>
    public virtual async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Aplica validações defensivas nos parâmetros de paginação (garante página mínima 1 e limita o tamanho máximo da página a 100 para evitar queries gigantes).
    /// Calcula o total de registros antes de aplicar o Skip/Take para que o frontend possa construir a navegação corretamente.
    /// </remarks>
    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        // Valida parâmetros defensivamente
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Proteção contra paginação gigante

        var query = _dbSet.AsNoTracking().AsQueryable();

        if (predicate is not null)
            query = query.Where(predicate);

        // Total ANTES de aplicar skip/take (necessário para o frontend saber quantas páginas existem)
        var totalCount = await query.CountAsync(cancellationToken);

        if (orderBy is not null)
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    // ──────────────────────────────────────────────
    // Escritas — Apenas marcam estado; SaveChanges é responsabilidade do UoW
    // ──────────────────────────────────────────────

    /// <inheritdoc />
    /// <remarks>
    /// Nenhum <c>SaveChanges</c> é executado aqui. O Unit of Work acumulará essa inserção com outras operações pendentes.
    /// </remarks>
    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>
    /// O uso do <c>Attach</c> garante que a entidade está anexada ao contexto. 
    /// Mudar o estado para <see cref="EntityState.Modified"/> marca todos os campos como alterados para o EF gerar o comando UPDATE corretamente.
    /// </remarks>
    public virtual void Update(T entity)
    {
        _dbSet.Attach(entity);
        context.Entry(entity).State = EntityState.Modified;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Aplica o padrão de <strong>Soft Delete</strong>: a entidade não é removida do banco, apenas tem a flag <c>IsDeleted</c> ativada e a data de <c>UpdatedAt</c> renovada.
    /// O Query Filter global do EF Core esconderá este registro automaticamente em queries futuras.
    /// </remarks>
    public virtual void Delete(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        Update(entity);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Remove o registro fisicamente do banco de dados chamando <c>_dbSet.Remove</c>. 
    /// Utilize apenas quando estritamente necessário (ex: cumprimento de requisições de deleção da LGPD / GDPR).
    /// </remarks>
    public virtual void HardDelete(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Reutiliza a lógica de soft delete iterando sobre a coleção.
    /// </remarks>
    public virtual void DeleteRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
            Delete(entity);
    }
}