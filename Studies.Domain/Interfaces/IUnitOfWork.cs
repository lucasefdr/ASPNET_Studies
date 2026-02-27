namespace Studies.Domain.Interfaces;


/// <summary>
/// Representa o "maestro" de todas as operações de banco de dados, implementando o padrão Unit of Work.
/// </summary>
/// <remarks>
/// A ideia central é que um único <c>SaveChangesAsync()</c> confirme TUDO o que aconteceu 
/// nos repositórios durante aquela operação de negócio, garantindo a integridade e a consistência dos dados.
/// </remarks>
/// <example>
/// Uso típico em um serviço de domínio ou aplicação:
/// <code>
/// await _uow.Orders.AddAsync(order);
/// _uow.Products.Update(product); // Deduz o estoque (operação síncrona no rastreamento)
/// await _uow.CommitAsync();      // Persiste os dois juntos no banco — ou nenhum em caso de falha
/// </code>
/// </example>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    // ── Repositórios expostos pelo UoW ────────────────────────────────────────

    /// <summary>
    /// Obtém a instância do repositório responsável pelas operações de produtos.
    /// </summary>
    IProductRepository Products { get; }

    // ── Controle de transação ────────────────────────────────────────

    /// <summary>
    /// Persiste todas as mudanças acumuladas (inserções, atualizações e exclusões) nos repositórios no banco de dados.
    /// </summary>
    /// <param name="cancellationToken">Token para monitorar requisições de cancelamento.</param>
    /// <returns>O número de entidades afetadas no banco de dados.</returns>
    /// <remarks>
    /// O Entity Framework Core já encapsula o <c>SaveChanges</c> em uma transação implícita. 
    /// Se uma exceção for lançada durante este processo, nenhuma alteração será salva no banco.
    /// </remarks>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Descarta todas as mudanças atualmente rastreadas pelo contexto, revertendo as entidades ao seu estado original.
    /// </summary>
    /// <remarks>
    /// Extremamente útil em cenários de erro e blocos <c>catch</c> onde é necessário limpar o estado 
    /// de rastreamento (Change Tracker) do Entity Framework sem persistir as falhas.
    /// </remarks>
    void Rollback();

    /// <summary>
    /// Executa um bloco de código dentro de uma transação explícita do banco de dados (BEGIN TRANSACTION ... COMMIT).
    /// </summary>
    /// <param name="operation">O bloco de código assíncrono (delegate) a ser executado dentro da transação.</param>
    /// <param name="cancellationToken">Token para monitorar requisições de cancelamento.</param>
    /// <returns>Uma tarefa (Task) que representa a operação assíncrona.</returns>
    /// <remarks>
    /// Utilize este método quando precisar de garantias de isolamento mais fortes do que o padrão oferecido 
    /// pelo EF Core, ou quando precisar coordenar múltiplas chamadas ao <see cref="CommitAsync"/> dentro de uma mesma transação.
    /// </remarks>
    Task ExecuteInTransactionAsync(Func<Task> operation,
                                   CancellationToken cancellationToken = default);

    /// <summary>
    /// Executa um bloco de código que retorna um valor dentro de uma transação explícita do banco de dados.
    /// </summary>
    /// <typeparam name="TResult">O tipo de dado que será retornado pela operação.</typeparam>
    /// <param name="operation">O bloco de código assíncrono (delegate) a ser executado e que produzirá um resultado.</param>
    /// <param name="cancellationToken">Token para monitorar requisições de cancelamento.</param>
    /// <returns>O resultado <typeparamref name="TResult"/> produzido pela operação encapsulada na transação.</returns>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default);

}
