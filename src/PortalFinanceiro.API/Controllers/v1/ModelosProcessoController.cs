using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Authorization;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/modelos-processos")]
[Authorize]
public class ModelosProcessoController : BaseController
{
    private readonly IModeloProcessoAppService _service;

    public ModelosProcessoController(IModeloProcessoAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool? ativo)
    {
        var result = await _service.ListarAsync(ObterIdUsuario(), ativo);
        return ApiResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(Guid id)
    {
        var result = await _service.ObterPorIdAsync(id, ObterIdUsuario());
        return ApiResponse(result);
    }

    [HttpPost]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> Criar([FromBody] ModeloProcessoRequest request)
    {
        var result = await _service.AdicionarAsync(ObterIdUsuario(), request);
        return ApiResponse(result, 201);
    }

    [HttpPut("{id}")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] ModeloProcessoRequest request)
    {
        var result = await _service.AtualizarAsync(id, ObterIdUsuario(), request);
        return ApiResponse(result);
    }

    [HttpDelete("{id}")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var result = await _service.ExcluirAsync(id, ObterIdUsuario());
        return ApiResponse(result);
    }

    [HttpPost("{id}/duplicar")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> Duplicar(Guid id)
    {
        var result = await _service.DuplicarAsync(id, ObterIdUsuario());
        return ApiResponse(result, 201);
    }

    [HttpPost("{id}/etapas")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> CriarEtapa(Guid id, [FromBody] ModeloEtapaRequest request)
    {
        var result = await _service.AdicionarEtapaAsync(id, ObterIdUsuario(), request);
        return ApiResponse(result, 201);
    }

    [HttpPut("{id}/etapas/{etapaId}")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> AtualizarEtapa(Guid id, Guid etapaId, [FromBody] ModeloEtapaRequest request)
    {
        var result = await _service.AtualizarEtapaAsync(id, etapaId, ObterIdUsuario(), request);
        return ApiResponse(result);
    }

    [HttpPut("{id}/etapas/{etapaId}/mover")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> MoverEtapa(Guid id, Guid etapaId, [FromQuery] int direcao)
    {
        var result = await _service.MoverEtapaAsync(id, etapaId, ObterIdUsuario(), direcao);
        return ApiResponse(result);
    }

    [HttpDelete("{id}/etapas/{etapaId}")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> ExcluirEtapa(Guid id, Guid etapaId)
    {
        var result = await _service.ExcluirEtapaAsync(id, etapaId, ObterIdUsuario());
        return ApiResponse(result);
    }

    [HttpPost("{id}/etapas/{etapaId}/itens")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> CriarItem(Guid id, Guid etapaId, [FromBody] ModeloItemRequest request)
    {
        var result = await _service.AdicionarItemAsync(id, etapaId, ObterIdUsuario(), request);
        return ApiResponse(result, 201);
    }

    [HttpPut("{id}/itens/{itemId}")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> AtualizarItem(Guid id, Guid itemId, [FromBody] ModeloItemRequest request)
    {
        var result = await _service.AtualizarItemAsync(id, itemId, ObterIdUsuario(), request);
        return ApiResponse(result);
    }

    [HttpPut("{id}/itens/{itemId}/mover")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> MoverItem(Guid id, Guid itemId, [FromQuery] int direcao)
    {
        var result = await _service.MoverItemAsync(id, itemId, ObterIdUsuario(), direcao);
        return ApiResponse(result);
    }

    [HttpDelete("{id}/itens/{itemId}")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> ExcluirItem(Guid id, Guid itemId)
    {
        var result = await _service.ExcluirItemAsync(id, itemId, ObterIdUsuario());
        return ApiResponse(result);
    }
}
