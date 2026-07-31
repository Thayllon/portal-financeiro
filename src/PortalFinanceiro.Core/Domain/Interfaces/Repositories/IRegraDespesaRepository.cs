using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IRegraDespesaRepository
{
    Task<RegraDespesa?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<RegraDespesa>> ListarPorUsuarioAsync(Guid idUsuario);
    Task InserirAsync(RegraDespesa entity);
    Task AtualizarAsync(RegraDespesa entity);
}
