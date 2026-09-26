using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Authorization;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequerPermissaoEscritaPessoaAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        if (PermissaoEscritaHelper.EhAdmin(user))
        {
            await next();
            return;
        }

        var idUsuario = PermissaoEscritaHelper.ObterIdUsuario(user);
        if (idUsuario is null)
        {
            context.Result = PermissaoEscritaHelper.ErroResult(Erro.Permissao("USUARIO_NAO_IDENTIFICADO", "Usuário não identificado."));
            return;
        }

        var request = context.ActionArguments.Values.OfType<PessoaRequest>().FirstOrDefault();
        if (request is null)
        {
            context.Result = PermissaoEscritaHelper.ErroResult(Erro.Validacao("REQUISICAO_INVALIDA", "Corpo da requisição é obrigatório."));
            return;
        }

        var modulo = request.Tipo == TipoPessoa.Parceiro ? "parceiros" : "clientes";
        var negado = PermissaoEscritaHelper.ErroResult(Erro.Permissao("PERMISSAO_ESCRITA_NEGADA", $"Você não tem permissão de escrita no módulo \"{modulo}\"."));
        var resultado = await PermissaoEscritaHelper.VerificarAsync(context.HttpContext, idUsuario.Value, modulo, negado);
        if (resultado is not null)
        {
            context.Result = resultado;
            return;
        }

        await next();
    }
}