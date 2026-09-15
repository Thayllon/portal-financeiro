using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IDespesaServicoRepository
{
    Task<IEnumerable<DespesaServico>> ListarPorDespesaAsync(Guid despesaId);
    Task InserirAsync(DespesaServico entity);
    Task InserirEmMassaAsync(IEnumerable<DespesaServico> entities);
    Task ExcluirPorDespesaAsync(Guid despesaId);
}
