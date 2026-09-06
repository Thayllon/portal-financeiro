using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.Core.Domain.Interfaces.Repositories;

public interface IPermissaoUsuarioRepository
{
    Task<IEnumerable<PermissaoUsuario>> ObterPorUsuarioIdAsync(Guid usuarioId);
    Task<PermissaoUsuario?> ObterPorUsuarioEModuloAsync(Guid usuarioId, string modulo);
    Task InserirAsync(PermissaoUsuario entity);
    Task AtualizarAsync(PermissaoUsuario entity);
    Task ExcluirAsync(Guid id);
    Task ExcluirPorUsuarioIdAsync(Guid usuarioId);
}
