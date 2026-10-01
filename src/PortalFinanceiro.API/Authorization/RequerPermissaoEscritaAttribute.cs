using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Authorization;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequerPermissaoEscritaAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _modulo;

    public RequerPermissaoEscritaAttribute(string modulo)
    {
        _modulo = modulo;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (PermissaoEscritaHelper.EhAdmin(user))
            return;

        var idUsuario = PermissaoEscritaHelper.ObterIdUsuario(user);
        if (idUsuario is null)
        {
            context.Result = PermissaoEscritaHelper.ErroResult(Erro.Permissao("USUARIO_NAO_IDENTIFICADO", "Usuário não identificado."));
            return;
        }

        var negado = PermissaoEscritaHelper.ErroResult(Erro.Permissao("PERMISSAO_ESCRITA_NEGADA", $"Você não tem permissão de escrita no módulo \"{_modulo}\"."));
        context.Result = await PermissaoEscritaHelper.VerificarAsync(context.HttpContext, idUsuario.Value, _modulo, negado);
    }
}