using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/categorias/servicos")]
[Authorize]
public class CategoriasServicosController : BaseController
{
    private readonly ICategoriaServicoAppService _service;

    public CategoriasServicosController(ICategoriaServicoAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var result = await _service.ListarAsync(ObterIdUsuario(), User.IsInRole("Admin"));
        return ApiResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(Guid id)
    {
        var result = await _service.ObterPorIdAsync(id, ObterIdUsuario(), User.IsInRole("Admin"));
        return ApiResponse(result);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CategoriaRequest request)
    {
        var result = await _service.AdicionarAsync(ObterIdUsuario(), request);
        return ApiResponse(result, 201);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CategoriaRequest request)
    {
        var result = await _service.AtualizarAsync(id, ObterIdUsuario(), User.IsInRole("Admin"), request);
        return ApiResponse(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var result = await _service.ExcluirAsync(id, ObterIdUsuario(), User.IsInRole("Admin"));
        return ApiResponse(result);
    }
}
