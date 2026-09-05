using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IReceitaServicoRepository
{
    Task<IEnumerable<ReceitaServico>> ListarPorReceitaAsync(Guid receitaId);
    Task InserirAsync(ReceitaServico entity);
    Task InserirEmMassaAsync(IEnumerable<ReceitaServico> entities);
    Task ExcluirPorReceitaAsync(Guid receitaId);
}
