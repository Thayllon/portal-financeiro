using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IPermissaoUsuarioAppService
{
    Task<Result<IEnumerable<PermissaoUsuarioResponse>>> ListarPorUsuarioAsync(Guid usuarioId);
    Task<Result<Unit>> SalvarPermissoesAsync(Guid usuarioId, IEnumerable<PermissaoUsuarioRequest> permissoes);
    Task<Result<bool>> VerificarPermissaoAsync(Guid usuarioId, string modulo, NivelPermissao nivelMinimo);
}
