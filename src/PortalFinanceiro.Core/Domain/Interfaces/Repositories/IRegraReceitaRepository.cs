using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Projections;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IRegraReceitaRepository
{
    Task<RegraReceita?> ObterPorIdAsync(Guid id);
    Task<RegraReceitaProjecao?> ObterProjecaoPorIdAsync(Guid id);
    Task<IEnumerable<RegraReceitaProjecao>> ListarPorUsuarioAsync(Guid idUsuario);
    Task InserirAsync(RegraReceita entity);
    Task AtualizarAsync(RegraReceita entity);
}