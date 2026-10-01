using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Authorization;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/contas-bancarias")]
[Authorize]
public class ContasBancariasController : BaseController
{
    private readonly IContaBancariaAppService _service;

    public ContasBancariasController(IContaBancariaAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var result = await _service.ListarAsync(ObterIdUsuario());
        return ApiResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(Guid id)
    {
        var result = await _service.ObterPorIdAsync(id, ObterIdUsuario());
        return ApiResponse(result);
    }

    [HttpPost]
    [RequerPermissaoEscrita("contas")]
    public async Task<IActionResult> Criar([FromBody] ContaBancariaRequest request)
    {
        var result = await _service.AdicionarAsync(ObterIdUsuario(), request);
        return ApiResponse(result, 201);
    }

    [HttpPut("{id}")]
    [RequerPermissaoEscrita("contas")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] ContaBancariaRequest request)
    {
        var result = await _service.AtualizarAsync(id, ObterIdUsuario(), request);
        return ApiResponse(result);
    }

    [HttpPut("{id}/padrao")]
    [RequerPermissaoEscrita("contas")]
    public async Task<IActionResult> DefinirPadrao(Guid id)
    {
        var result = await _service.DefinirPadraoAsync(id, ObterIdUsuario());
        return ApiResponse(result);
    }

    [HttpDelete("{id}")]
    [RequerPermissaoEscrita("contas")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var result = await _service.ExcluirAsync(id, ObterIdUsuario());
        return ApiResponse(result);
    }
}
