using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IRegraReceitaRepository
{
    Task<RegraReceita?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<RegraReceita>> ListarPorUsuarioAsync(Guid idUsuario);
    Task InserirAsync(RegraReceita entity);
    Task AtualizarAsync(RegraReceita entity);
}
