using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/contratos")]
[Authorize]
public class ContratosController : BaseController
{
    private readonly IContratoAppService _service;
    private readonly IReceitaAppService _receitaService;

    public ContratosController(IContratoAppService service, IReceitaAppService receitaService)
    {
        _service = service;
        _receitaService = receitaService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool? ativo, [FromQuery] bool? ehRecorrente)
    {
        var result = await _service.ListarAsync(ObterIdUsuario(), ativo, ehRecorrente);
        return ApiResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(Guid id)
    {
        var result = await _service.ObterPorIdAsync(id, ObterIdUsuario());
        return ApiResponse(result);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ContratoRequest request)
    {
        var result = await _service.AdicionarAsync(ObterIdUsuario(), request);
        return ApiResponse(result, 201);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] ContratoRequest request)
    {
        var result = await _service.AtualizarAsync(id, ObterIdUsuario(), request);
        return ApiResponse(result);
    }

    [HttpPut("{id}/encerrar")]
    public async Task<IActionResult> Encerrar(Guid id)
    {
        var result = await _service.EncerrarAsync(id, ObterIdUsuario());
        return ApiResponse(result);
    }

    [HttpPut("{id}/reativar")]
    public async Task<IActionResult> Reativar(Guid id)
    {
        var result = await _service.ReativarAsync(id, ObterIdUsuario());
        return ApiResponse(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var result = await _service.ExcluirAsync(id, ObterIdUsuario());
        return ApiResponse(result);
    }

    [HttpGet("{id}/receitas")]
    public async Task<IActionResult> ListarReceitas(Guid id)
    {
        var result = await _receitaService.ListarPorContratoAsync(ObterIdUsuario(), id);
        return ApiResponse(result);
    }
}
