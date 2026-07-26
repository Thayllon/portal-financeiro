using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/v{version:apiVersion}/despesas-mensais")]
[Authorize]
public class DespesasMensaisController : BaseController
{
    private readonly IDespesaMensalAppService _service;

    public DespesasMensaisController(IDespesaMensalAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ListarPorMes([FromQuery] Guid idUsuario, [FromQuery] int mes, [FromQuery] int ano)
    {
        var result = await _service.ListarPorMesAsync(idUsuario, mes, ano);
        return ApiResponse(result);
    }

    [HttpPost("{id}/pagar")]
    public async Task<IActionResult> Pagar(Guid id, [FromBody] MensalStatusRequest request)
    {
        var result = await _service.PagarAsync(id, request);
        return ApiResponse(result);
    }

    [HttpPost("{id}/estornar")]
    public async Task<IActionResult> Estornar(Guid id)
    {
        var result = await _service.EstornarAsync(id);
        return ApiResponse(result);
    }
}
