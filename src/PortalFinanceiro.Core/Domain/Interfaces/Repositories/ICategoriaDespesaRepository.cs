using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface ICategoriaDespesaRepository
{
    Task<CategoriaDespesa?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<CategoriaDespesa>> ListarPorUsuarioAsync(Guid idUsuario);
    Task InserirAsync(CategoriaDespesa entity);
    Task AtualizarAsync(CategoriaDespesa entity);
}
