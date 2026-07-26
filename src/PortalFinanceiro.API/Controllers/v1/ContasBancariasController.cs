using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/v{version:apiVersion}/contas-bancarias")]
[Authorize]
public class ContasBancariasController : BaseController
{
    private readonly IContaBancariaAppService _service;

    public ContasBancariasController(IContaBancariaAppService service)
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

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ContaBancariaRequest request, [FromQuery] Guid idUsuario)
    {
        var result = await _service.AdicionarAsync(idUsuario, request);
        return ApiResponse(result, 201);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] ContaBancariaRequest request)
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
