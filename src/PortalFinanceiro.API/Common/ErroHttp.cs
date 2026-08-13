using System.Net;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Common;

public static class ErroHttp
{
    public static HttpStatusCode ObterStatus(ETipoErro tipo) => tipo switch
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
}