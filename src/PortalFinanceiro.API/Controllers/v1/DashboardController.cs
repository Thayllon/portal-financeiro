using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/v{version:apiVersion}/dashboard")]
[Authorize]
public class DashboardController : BaseController
{
    private readonly IDashboardAppService _service;

    public DashboardController(IDashboardAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Obter([FromQuery] Guid idUsuario, [FromQuery] int mes, [FromQuery] int ano)
    {
        var result = await _service.ObterDashboardAsync(idUsuario, mes, ano);
        return ApiResponse(result);
    }
}
