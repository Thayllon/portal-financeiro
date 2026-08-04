using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface ICategoriaDespesaRepository
{
    Task<CategoriaDespesa?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<CategoriaDespesa>> ListarPorUsuarioAsync(Guid idUsuario);
    Task<IEnumerable<CategoriaDespesa>> ListarPorPaiAsync(Guid? categoriaPaiId);
    Task InserirAsync(CategoriaDespesa entity);
    Task AtualizarAsync(CategoriaDespesa entity);
    Task ExcluirAsync(Guid id);
}
