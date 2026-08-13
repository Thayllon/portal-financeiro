using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Projections;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IRegraDespesaRepository
{
    Task<RegraDespesa?> ObterPorIdAsync(Guid id);
    Task<RegraDespesaProjecao?> ObterProjecaoPorIdAsync(Guid id);
    Task<IEnumerable<RegraDespesaProjecao>> ListarPorUsuarioAsync(Guid idUsuario);
    Task InserirAsync(RegraDespesa entity);
    Task AtualizarAsync(RegraDespesa entity);
}