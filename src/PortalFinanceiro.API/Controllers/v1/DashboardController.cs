using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/dashboard")]
[Authorize]
public class DashboardController : BaseController
{
    private readonly IDashboardAppService _service;

    public DashboardController(IDashboardAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Obter([FromQuery] int mes, [FromQuery] int ano)
    {
        var result = await _service.ObterDashboardAsync(ObterIdUsuario(), mes, ano);
        return ApiResponse(result);
    }
}
