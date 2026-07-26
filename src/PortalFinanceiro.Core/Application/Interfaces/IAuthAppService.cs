using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IAuthAppService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    Task<Result<UsuarioResponse>> RegistrarAsync(UsuarioRequest request);
}
