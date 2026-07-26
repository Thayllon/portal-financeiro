using System.Net;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
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
            ETipoErro.Timeout => HttpStatusCode.GatewayTimeout,
            ETipoErro.Externo => HttpStatusCode.BadGateway,
            _ => HttpStatusCode.InternalServerError
        };

        return StatusCode((int)httpCode, erro);
    }
}
