using PortalFinanceiro.Core.Application.Dtos.Response;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IDiagnosticoAppService
{
    Task<DiagnosticoResponse> GerarAsync(Guid idUsuario);
}
