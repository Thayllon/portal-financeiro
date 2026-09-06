using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;

namespace PortalFinanceiro.API.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissaoUsuarioAppService _permissaoService;

    public PermissionHandler(IPermissaoUsuarioAppService permissaoService)
    {
        _permissaoService = permissaoService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
            return;

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
            return;

        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var resultado = await _permissaoService.VerificarPermissaoAsync(
            userId, requirement.Modulo, (NivelPermissao)requirement.NivelMinimo);

        if (resultado.Dado == true)
            context.Succeed(requirement);
    }
}
