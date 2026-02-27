using Studies.Domain.Shared;
using System.Linq.Expressions;

namespace Studies.Domain.Interfaces;

/// <summary>
/// Define o contrato genérico para os repositórios da aplicação.
/// Abstrai o acesso a dados para entidades, garantindo o acesso a propriedades comuns de auditoria e exclusão lógica.
/// </summary>
/// <typeparam name="T">O tipo de entidade manipulada pelo repositório. Deve implementar <see cref="IAggregateRoot"/>.</typeparam>
public interface IRepository<T> where T : EntityBase, IAggregateRoot
{
    // ──────────────────────────────────────────────
    // Leituras
    // ──────────────────────────────────────────────

    IQueryable<T> Get();

    /// <summary>
    /// Obtém uma entidade de forma assíncrona com base no seu identificador único.
    /// </summary>
    /// <param name="id">O identificador único da entidade a ser buscada.</param>
    /// <param name="cancellationToken">Token para monitorar requisições de cancelamento.</param>
    /// <returns>A entidade encontrada, ou <c>null</c> caso não exista.</returns>
    Task<T?> GetByIdAsync(int id,
                          CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todas as entidades ativas da base de dados, ignorando registros que sofreram exclusão lógica (soft-delete).
    /// </summary>
    /// <param name="cancellationToken">Token para monitorar requisições de cancelamento.</param>
    /// <returns>Uma coleção contendo todas as entidades ativas.</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca entidades de forma assíncrona que satisfaçam a condição especificada pelo predicado.
    /// </summary>
    /// <param name="predicate">A expressão lambda contendo a condição de busca.</param>
    /// <param name="cancellationToken">Token para monitorar requisições de cancelamento.</param>
    /// <returns>Uma coleção de entidades que atendem aos critérios informados.</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate,
                                   CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém a única entidade que satisfaz a condição especificada, ou <c>null</c> se nenhuma for encontrada.
    /// </summary>
    /// <param name="predicate">A expressão lambda contendo a condição de busca.</param>
    /// <param name="cancellationToken">Token para monitorar requisições de cancelamento.</param>
    /// <returns>A entidade encontrada ou <c>null</c>.</returns>
    /// <exception cref="InvalidOperationException">Lançada se mais de um elemento satisfizer a condição da busca.</exception>
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate,
                                  CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica de forma assíncrona e otimizada se existe pelo menos uma entidade que satisfaça a condição especificada, sem carregá-la na memória.
    /// </summary>
    /// <param name="predicate">A expressão lambda contendo a condição de busca.</param>
    /// <param name="cancellationToken">Token para monitorar requisições de cancelamento.</param>
    /// <returns><c>true</c> se pelo menos uma entidade existir; caso contrário, <c>false</c>.</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate,
                           CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna a quantidade de entidades que satisfazem a condição especificada. 
    /// Caso nenhuma condição seja informada, retorna o total de registros ativos.
    /// </summary>
    /// <param name="predicate">Condição de filtro opcional.</param>
    /// <param name="cancellationToken">Token para monitorar requisições de cancelamento.</param>
    /// <returns>O número total de elementos que atendem aos critérios.</returns>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null,
                         CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma lista paginada de entidades, permitindo filtros e ordenação.
    /// </summary>
    /// <param name="pageNumber">O número da página atual (iniciando em 1).</param>
    /// <param name="pageSize">A quantidade de registros por página.</param>
    /// <param name="predicate">Condição de filtro opcional.</param>
    /// <param name="orderBy">Expressão opcional para ordenação dos dados.</param>
    /// <param name="ascending">Define se a ordenação deve ser crescente (<c>true</c>) ou decrescente (<c>false</c>).</param>
    /// <param name="cancellationToken">Token para monitorar requisições de cancelamento.</param>
    /// <returns>Uma tupla contendo a coleção de itens da página (<c>Items</c>) e o total geral de registros que satisfazem o filtro (<c>TotalCount</c>).</returns>
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default);

    // ──────────────────────────────────────────────
    // Escritas — Observação: A persistência física (SaveChanges) deve ser delegada ao Unit of Work.
    // ──────────────────────────────────────────────

    /// <summary>
    /// Adiciona uma nova entidade ao rastreamento do contexto de dados. 
    /// O salvamento no banco de dados não ocorre imediatamente.
    /// </summary>
    /// <param name="entity">A entidade a ser adicionada.</param>
    /// <param name="cancellationToken">Token para monitorar requisições de cancelamento.</param>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma coleção de entidades ao rastreamento do contexto de dados de uma só vez (bulk insert em memória).
    /// </summary>
    /// <param name="entities">A coleção de entidades a serem adicionadas.</param>
    /// <param name="cancellationToken">Token para monitorar requisições de cancelamento.</param>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca a entidade fornecida como modificada no contexto de dados.
    /// </summary>
    /// <param name="entity">A entidade com as modificações a serem rastreadas.</param>
    void Update(T entity);

    /// <summary>
    /// Realiza a exclusão lógica (soft delete) da entidade, marcando a propriedade de deleção e atualizando a data de modificação.
    /// A entidade não é removida fisicamente do banco de dados.
    /// </summary>
    /// <param name="entity">A entidade a ser excluída logicamente.</param>
    void Delete(T entity);

    /// <summary>
    /// Realiza a exclusão física da entidade, removendo-a permanentemente do banco de dados. 
    /// Recomenda-se cautela ao utilizar este método.
    /// </summary>
    /// <param name="entity">A entidade a ser removida definitivamente.</param>
    void HardDelete(T entity);

    /// <summary>
    /// Realiza a exclusão lógica (soft delete) de uma coleção de entidades em lote.
    /// </summary>
    /// <param name="entities">A coleção de entidades a serem excluídas logicamente.</param>
    void DeleteRange(IEnumerable<T> entities);
}
