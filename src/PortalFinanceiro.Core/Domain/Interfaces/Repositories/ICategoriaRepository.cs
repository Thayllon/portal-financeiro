namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface ICategoriaRepository<T> where T : class
{
    Task<T?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<T>> ListarAsync();
    Task<IEnumerable<T>> ListarPorPaiAsync(Guid? categoriaPaiId);
    Task InserirAsync(T entity);
    Task AtualizarAsync(T entity);
    Task ExcluirAsync(Guid id);
}
