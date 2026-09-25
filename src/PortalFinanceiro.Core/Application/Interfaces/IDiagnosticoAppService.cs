using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IDiagnosticoAppService
{
    Task<Result<DiagnosticoResponse>> GerarAsync(Guid idUsuario);
}
