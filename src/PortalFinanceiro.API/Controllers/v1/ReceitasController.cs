using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/[controller]")]
[Authorize]
public class ReceitasController : BaseController
{
    private readonly IReceitaAppService _service;

    public ReceitasController(IReceitaAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int mes, [FromQuery] int ano, [FromQuery] Guid? idConta = null, [FromQuery] int? status = null, [FromQuery] Guid? idCategoria = null, [FromQuery] string? busca = null)
    {
        var result = await _service.ListarAsync(ObterIdUsuario(), mes, ano, idConta, status, idCategoria, busca);
        return ApiResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(Guid id)
    {
        var result = await _service.ObterPorIdAsync(id);
        return ApiResponse(result);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ReceitaRequest request)
    {
        var result = await _service.AdicionarAsync(ObterIdUsuario(), request);
        return ApiResponse(result, 201);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] ReceitaRequest request)
    {
        var result = await _service.AtualizarAsync(id, request);
        return ApiResponse(result);
    }

    [HttpPost("{id}/receber")]
    public async Task<IActionResult> Receber(Guid id, [FromBody] MensalStatusRequest request)
    {
        var result = await _service.ReceberAsync(id, request);
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
