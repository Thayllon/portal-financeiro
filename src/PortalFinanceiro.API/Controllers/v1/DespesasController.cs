using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/[controller]")]
[Authorize]
public class DespesasController : BaseController
{
    private readonly IDespesaAppService _service;

    public DespesasController(IDespesaAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] Guid idUsuario, [FromQuery] int mes, [FromQuery] int ano, [FromQuery] Guid? idConta = null, [FromQuery] string? status = null, [FromQuery] Guid? idCategoria = null, [FromQuery] string? busca = null)
    {
        var result = await _service.ListarAsync(idUsuario, mes, ano, idConta, status, idCategoria, busca);
        return ApiResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(Guid id)
    {
        var result = await _service.ObterPorIdAsync(id);
        return ApiResponse(result);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] DespesaRequest request, [FromQuery] Guid idUsuario)
    {
        var result = await _service.AdicionarAsync(idUsuario, request);
        return ApiResponse(result, 201);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] DespesaRequest request)
    {
        var result = await _service.AtualizarAsync(id, request);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var result = await _service.ExcluirAsync(id);
        return ApiResponse(result);
    }
}
