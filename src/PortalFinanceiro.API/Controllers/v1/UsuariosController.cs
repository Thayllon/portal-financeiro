using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsuariosController : BaseController
{
    private readonly IUsuarioAppService _service;

    public UsuariosController(IUsuarioAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var result = await _service.ListarAsync();
        return ApiResponse(result);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] UsuarioRequest request)
    {
        var result = await _service.AdicionarAsync(request);
        return ApiResponse(result, 201);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] UsuarioRequest request)
    {
        var result = await _service.AtualizarAsync(id, request);
        return ApiResponse(result);
    }

    [HttpPatch("{id}/ativo")]
    public async Task<IActionResult> AlterarAtivo(Guid id, [FromBody] bool ativo)
    {
        var result = await _service.AlterarAtivoAsync(id, ativo);
        return ApiResponse(result);
    }
}
