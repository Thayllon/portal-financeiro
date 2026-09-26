using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Authorization;

public static class PermissaoEscritaHelper
{
    public static Guid? ObterIdUsuario(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");
        if (claim is null || !Guid.TryParse(claim.Value, out var id))
            return null;
        return id;
    }

    public static bool EhAdmin(ClaimsPrincipal user)
        => user.IsInRole("Admin");

    public static async Task<IActionResult?> VerificarAsync(HttpContext httpContext, Guid idUsuario, string modulo, IActionResult negado)
    {
        var permissoes = httpContext.RequestServices.GetService(typeof(IPermissaoUsuarioAppService)) as IPermissaoUsuarioAppService;
        if (permissoes is null)
            return ErroResult(Erro.Infraestrutura("Serviço de permissões não configurado."));

        var verificado = await permissoes.VerificarPermissaoAsync(idUsuario, modulo, NivelPermissao.Escrita);
        if (!verificado.EhSucesso || !verificado.Dado)
            return negado;
        return null;
    }

    public static IActionResult ErroResult(Erro erro)
        => new ObjectResult(erro) { StatusCode = (int)PortalFinanceiro.API.Common.ErroHttp.ObterStatus(erro.Tipo) };
}