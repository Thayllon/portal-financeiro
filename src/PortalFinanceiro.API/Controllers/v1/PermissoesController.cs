using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/usuarios/{usuarioId}/permissoes")]
[Authorize(Roles = "Admin")]
public class PermissoesController : BaseController
{
    private readonly IPermissaoUsuarioAppService _service;

    public PermissoesController(IPermissaoUsuarioAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(Guid usuarioId)
    {
        var result = await _service.ListarPorUsuarioAsync(usuarioId);
        return ApiResponse(result);
    }

    [HttpPut]
    public async Task<IActionResult> Salvar(Guid usuarioId, [FromBody] IEnumerable<PermissaoUsuarioRequest> permissoes)
    {
        var result = await _service.SalvarPermissoesAsync(usuarioId, permissoes);
        return ApiResponse(result);
    }
}
