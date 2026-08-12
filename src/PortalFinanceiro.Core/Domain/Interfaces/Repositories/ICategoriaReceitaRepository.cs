using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface ICategoriaReceitaRepository
{
    Task<CategoriaReceita?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<CategoriaReceita>> ListarAsync();
    Task<IEnumerable<CategoriaReceita>> ListarPorPaiAsync(Guid? categoriaPaiId);
    Task InserirAsync(CategoriaReceita entity);
    Task AtualizarAsync(CategoriaReceita entity);
    Task ExcluirAsync(Guid id);
}
