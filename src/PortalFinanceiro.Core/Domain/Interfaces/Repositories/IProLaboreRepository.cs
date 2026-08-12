using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IProLaboreRepository
{
    Task<ProLabore?> ObterPorIdAsync(Guid id);
    Task<ProLabore?> ObterPorUsuarioMesAsync(Guid idUsuario, int mes, int ano);
    Task<IEnumerable<ProLabore>> ListarPorUsuarioAsync(Guid idUsuario);
    Task InserirAsync(ProLabore entity);
    Task AtualizarAsync(ProLabore entity);
    Task ExcluirAsync(Guid id);
}