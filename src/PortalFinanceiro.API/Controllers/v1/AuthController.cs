using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.API.Controllers;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Interfaces;

namespace PortalFinanceiro.API.Controllers.v1;

[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthAppService _authAppService;

    public AuthController(IAuthAppService authAppService)
    {
        _authAppService = authAppService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authAppService.LoginAsync(request);
        return ApiResponse(result);
    }

    /// <summary>Obtém um token do usuário administrador padrão de desenvolvimento (admin@portal.com / senhasenha).</summary>
    [HttpGet("token")]
    [AllowAnonymous]
    public async Task<IActionResult> ObterTokenDev()
    {
        var result = await _authAppService.LoginAsync(new LoginRequest
        {
            Email = "admin@portal.com",
            Senha = "senhasenha"
        });
        return ApiResponse(result);
    }
}
