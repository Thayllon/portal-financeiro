using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Authorization;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/processos")]
[Authorize]
public class ProcessosController : BaseController
{
    private readonly IProcessoAppService _service;

    public ProcessosController(IProcessoAppService service)
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
    public async Task<IActionResult> Criar([FromBody] ProcessoRequest request)
    {
        var result = await _service.AdicionarAsync(ObterIdUsuario(), request);
        return ApiResponse(result, 201);
    }

    [HttpPut("{id}")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] ProcessoRequest request)
    {
        var result = await _service.AtualizarAsync(id, ObterIdUsuario(), request);
        return ApiResponse(result);
    }

    [HttpPut("{id}/encerrar")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> Encerrar(Guid id)
    {
        var result = await _service.EncerrarAsync(id, ObterIdUsuario());
        return ApiResponse(result);
    }

    [HttpPut("{id}/reativar")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> Reativar(Guid id)
    {
        var result = await _service.ReativarAsync(id, ObterIdUsuario());
        return ApiResponse(result);
    }

    [HttpDelete("{id}")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var result = await _service.ExcluirAsync(id, ObterIdUsuario());
        return ApiResponse(result);
    }

    [HttpPost("{id}/etapas")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> CriarEtapa(Guid id, [FromBody] ProcessoEtapaRequest request)
    {
        var result = await _service.AdicionarEtapaAsync(id, ObterIdUsuario(), request);
        return ApiResponse(result, 201);
    }

    [HttpPut("{id}/etapas/{etapaId}")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> AtualizarEtapa(Guid id, Guid etapaId, [FromBody] ProcessoEtapaRequest request)
    {
        var result = await _service.AtualizarEtapaAsync(id, etapaId, ObterIdUsuario(), request);
        return ApiResponse(result);
    }

    [HttpPut("{id}/etapas/{etapaId}/concluir")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> ConcluirEtapa(Guid id, Guid etapaId)
    {
        var result = await _service.ConcluirEtapaAsync(id, etapaId, ObterIdUsuario());
        return ApiResponse(result);
    }

    [HttpPut("{id}/etapas/{etapaId}/estornar")]
    [RequerPermissaoEscrita("processos")]
    public async Task<IActionResult> EstornarEtapa(Guid id, Guid etapaId)
    {
        var result = await _service.EstornarEtapaAsync(id, etapaId, ObterIdUsuario());
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
}
