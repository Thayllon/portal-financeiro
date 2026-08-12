using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected Guid ObterIdUsuario()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null)
            throw new UnauthorizedAccessException("Token JWT não contém identificador do usuário.");
        return Guid.Parse(claim.Value);
    }

    protected IActionResult ApiResponse<T>(Result<T> result, int successStatusCode = 200)
    {
        if (result.EhSucesso)
        {
            return successStatusCode switch
            {
                201 => Created(string.Empty, result.Dado),
                _ => Ok(result.Dado)
            };
        }

        return ErrorResponse(result.Erro!);
    }

    private IActionResult ErrorResponse(Erro erro)
    {
        var httpCode = erro.Tipo switch
        {
            ETipoErro.Validacao => HttpStatusCode.BadRequest,
            ETipoErro.Negocio => HttpStatusCode.UnprocessableEntity,
            ETipoErro.NaoEncontrado => HttpStatusCode.NotFound,
            ETipoErro.Conflito => HttpStatusCode.Conflict,
            ETipoErro.Permissao => HttpStatusCode.Forbidden,
            ETipoErro.Timeout => HttpStatusCode.GatewayTimeout,
            ETipoErro.Externo => HttpStatusCode.BadGateway,
            _ => HttpStatusCode.InternalServerError
        };

        return StatusCode((int)httpCode, erro);
    }
}
