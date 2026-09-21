using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Projections;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IReceitaServicoRepository
{
    Task<IEnumerable<ReceitaServicoProjecao>> ListarPorReceitaAsync(Guid receitaId);
    Task InserirEmMassaAsync(IEnumerable<ReceitaServico> entities);
    Task ExcluirPorReceitaAsync(Guid receitaId);
}
