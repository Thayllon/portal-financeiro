using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/[controller]")]
[Authorize]
public class RegrasReceitasController : BaseController
{
    private readonly IRegraReceitaAppService _service;

    public RegrasReceitasController(IRegraReceitaAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] Guid idUsuario)
    {
        var result = await _service.ListarAsync(idUsuario);
        return ApiResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(Guid id)
    {
        var result = await _service.ObterPorIdAsync(id);
        return ApiResponse(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] RegraReceitaRequest request)
    {
        var result = await _service.AtualizarAsync(id, request);
        return ApiResponse(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var result = await _service.ExcluirAsync(id);
        return ApiResponse(result);
    }
}
