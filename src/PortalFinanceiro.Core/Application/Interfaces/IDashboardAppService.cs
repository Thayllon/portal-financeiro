using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.Core.Application.Interfaces;

public interface IDashboardAppService
{
    Task<Result<DashboardResponse>> ObterDashboardAsync(Guid idUsuario, int mes, int ano);
    Task<Result<DashboardAnualResponse>> ObterDashboardAnualAsync(Guid idUsuario, int ano, Guid? idConta = null);
}
