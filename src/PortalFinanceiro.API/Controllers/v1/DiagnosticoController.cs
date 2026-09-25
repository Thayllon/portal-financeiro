using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/diagnostico")]
[Authorize(Roles = "Admin")]
public class DiagnosticoController : BaseController
{
    private readonly IDiagnosticoAppService _service;

    public DiagnosticoController(IDiagnosticoAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Gerar()
        => ApiResponse(await _service.GerarAsync(ObterIdUsuario()));
}