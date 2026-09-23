using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Projections;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IContratoRepository
{
    Task<Contrato?> ObterPorIdAsync(Guid id);
    Task<ContratoProjecao?> ObterProjecaoPorIdAsync(Guid id);
    Task<IEnumerable<ContratoProjecao>> ListarAsync(Guid idUsuario, bool? ativo = null, bool? ehRecorrente = null);
    Task InserirAsync(Contrato entity);
    Task AtualizarAsync(Contrato entity);
    Task<decimal> SomarReceitasPorStatusAsync(Guid idContrato, int status);
}
