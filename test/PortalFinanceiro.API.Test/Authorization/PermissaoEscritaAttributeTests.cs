using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PortalFinanceiro.API.Authorization;
using PortalFinanceiro.Core.Application.Dtos.Request;
using PortalFinanceiro.Core.Application.Dtos.Response;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Domain.Entities;
using PortalFinanceiro.Core.Domain.Enums;
using PortalFinanceiro.Core.Domain.Results;

namespace PortalFinanceiro.API.Test;

public class RequerPermissaoEscritaAttributeTests
{
    private sealed class PermissaoFake : IPermissaoUsuarioAppService
    {
        private readonly NivelPermissao? _nivel;

        public PermissaoFake(NivelPermissao? nivel = null)
        {
            _nivel = nivel;
        }

        public Task<Result<IEnumerable<PermissaoUsuarioResponse>>> ListarPorUsuarioAsync(Guid usuarioId)
            => Task.FromResult(Result<IEnumerable<PermissaoUsuarioResponse>>.Sucesso(new List<PermissaoUsuarioResponse>()));

        public Task<Result<Unit>> SalvarPermissoesAsync(Guid usuarioId, IEnumerable<PermissaoUsuarioRequest> permissoes)
            => Task.FromResult(Result<Unit>.Sucesso(Unit.Value));

        public Task<Result<bool>> VerificarPermissaoAsync(Guid usuarioId, string modulo, NivelPermissao nivelMinimo)
            => Task.FromResult(Result<bool>.Sucesso(_nivel is not null && _nivel >= nivelMinimo));
    }

    private static AuthorizationFilterContext CriarContexto(ClaimsPrincipal user, object? request = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IPermissaoUsuarioAppService, PermissaoFake>();
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            User = user,
            RequestServices = serviceProvider
        };

        return new AuthorizationFilterContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>());
    }

    private static ClaimsPrincipal CriarUsuario(Guid idUsuario, bool admin = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, idUsuario.ToString())
        };
        if (admin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var identity = new ClaimsIdentity(claims, "Teste");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CriarUsuarioSemClaim()
    {
        var identity = new ClaimsIdentity("Teste");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task Admin_PassaMesmoSemPermissao()
    {
        var usuario = CriarUsuario(Guid.NewGuid(), admin: true);
        var contexto = CriarContexto(usuario);

        var atributo = new RequerPermissaoEscritaAttribute("receitas");
        await atributo.OnAuthorizationAsync(contexto);

        contexto.Result.Should().BeNull();
    }

    [Fact]
    public async Task UsuarioComEscrita_Passa()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IPermissaoUsuarioAppService>(new PermissaoFake(NivelPermissao.Escrita));
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            User = CriarUsuario(Guid.NewGuid()),
            RequestServices = serviceProvider
        };

        var contexto = new AuthorizationFilterContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>());

        var atributo = new RequerPermissaoEscritaAttribute("receitas");
        await atributo.OnAuthorizationAsync(contexto);

        contexto.Result.Should().BeNull();
    }

    [Fact]
    public async Task UsuarioComLeitura_Recebe403()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IPermissaoUsuarioAppService>(new PermissaoFake(NivelPermissao.Leitura));
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            User = CriarUsuario(Guid.NewGuid()),
            RequestServices = serviceProvider
        };

        var contexto = new AuthorizationFilterContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>());

        var atributo = new RequerPermissaoEscritaAttribute("receitas");
        await atributo.OnAuthorizationAsync(contexto);

        var resultado = contexto.Result.Should().BeOfType<ObjectResult>().Subject;
        resultado.StatusCode.Should().Be(403);
        resultado.Value.Should().BeAssignableTo<Erro>();
        var erro = (Erro)resultado.Value!;
        erro.Codigo.Should().Be("PERMISSAO_ESCRITA_NEGADA");
    }

    [Fact]
    public async Task UsuarioSemPermissao_Recebe403()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IPermissaoUsuarioAppService>(new PermissaoFake(null));
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            User = CriarUsuario(Guid.NewGuid()),
            RequestServices = serviceProvider
        };

        var contexto = new AuthorizationFilterContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>());

        var atributo = new RequerPermissaoEscritaAttribute("receitas");
        await atributo.OnAuthorizationAsync(contexto);

        var resultado = contexto.Result.Should().BeOfType<ObjectResult>().Subject;
        resultado.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task UsuarioSemClaimDeId_Recebe403()
    {
        var httpContext = new DefaultHttpContext
        {
            User = CriarUsuarioSemClaim(),
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };

        var contexto = new AuthorizationFilterContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>());

        var atributo = new RequerPermissaoEscritaAttribute("receitas");
        await atributo.OnAuthorizationAsync(contexto);

        var resultado = contexto.Result.Should().BeOfType<ObjectResult>().Subject;
        resultado.StatusCode.Should().Be(403);
    }
}

public class RequerPermissaoEscritaPessoaAttributeTests
{
    private sealed class PermissaoFake : IPermissaoUsuarioAppService
    {
        private readonly NivelPermissao? _nivel;
        private readonly string? _moduloEsperado;

        public PermissaoFake(NivelPermissao? nivel, string? moduloEsperado = null)
        {
            _nivel = nivel;
            _moduloEsperado = moduloEsperado;
        }

        public Task<Result<IEnumerable<PermissaoUsuarioResponse>>> ListarPorUsuarioAsync(Guid usuarioId)
            => Task.FromResult(Result<IEnumerable<PermissaoUsuarioResponse>>.Sucesso(new List<PermissaoUsuarioResponse>()));

        public Task<Result<Unit>> SalvarPermissoesAsync(Guid usuarioId, IEnumerable<PermissaoUsuarioRequest> permissoes)
            => Task.FromResult(Result<Unit>.Sucesso(Unit.Value));

        public Task<Result<bool>> VerificarPermissaoAsync(Guid usuarioId, string modulo, NivelPermissao nivelMinimo)
            => Task.FromResult(Result<bool>.Sucesso(_moduloEsperado == modulo && _nivel is not null && _nivel >= nivelMinimo));
    }

    private static (ActionExecutingContext contexto, ActionExecutionDelegate proximo) CriarContexto(
        ClaimsPrincipal user, PessoaRequest request, string moduloEsperado, NivelPermissao? nivel)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IPermissaoUsuarioAppService>(new PermissaoFake(nivel, moduloEsperado));
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            User = user,
            RequestServices = serviceProvider
        };

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var executando = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?> { ["request"] = request },
            controller: null);

        Task<ActionExecutedContext> ActionExecutionDelegate()
            => Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null));

        return (executando, ActionExecutionDelegate);
    }

    private static ClaimsPrincipal CriarUsuario(Guid idUsuario)
        => new(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()) },
            "Teste"));

    [Fact]
    public async Task Cliente_ValidaModuloClientes()
    {
        var (contexto, proximo) = CriarContexto(
            CriarUsuario(Guid.NewGuid()),
            new PessoaRequest { Nome = "João", Tipo = TipoPessoa.Cliente },
            "clientes",
            NivelPermissao.Escrita);

        var atributo = new RequerPermissaoEscritaPessoaAttribute();
        await atributo.OnActionExecutionAsync(contexto, proximo);

        contexto.Result.Should().BeNull();
    }

    [Fact]
    public async Task Parceiro_ValidaModuloParceiros()
    {
        var (contexto, proximo) = CriarContexto(
            CriarUsuario(Guid.NewGuid()),
            new PessoaRequest { Nome = "Empresa", Tipo = TipoPessoa.Parceiro },
            "parceiros",
            NivelPermissao.Escrita);

        var atributo = new RequerPermissaoEscritaPessoaAttribute();
        await atributo.OnActionExecutionAsync(contexto, proximo);

        contexto.Result.Should().BeNull();
    }

    [Fact]
    public async Task ClienteComLeitura_Recebe403()
    {
        var (contexto, proximo) = CriarContexto(
            CriarUsuario(Guid.NewGuid()),
            new PessoaRequest { Nome = "João", Tipo = TipoPessoa.Cliente },
            "clientes",
            NivelPermissao.Leitura);

        var atributo = new RequerPermissaoEscritaPessoaAttribute();
        await atributo.OnActionExecutionAsync(contexto, proximo);

        var resultado = contexto.Result.Should().BeOfType<ObjectResult>().Subject;
        resultado.StatusCode.Should().Be(403);
        var erro = (Erro)resultado.Value!;
        erro.Codigo.Should().Be("PERMISSAO_ESCRITA_NEGADA");
    }

    [Fact]
    public async Task SemBody_RecebeValidacao()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IPermissaoUsuarioAppService>(new PermissaoFake(NivelPermissao.Escrita, "clientes"));
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            User = CriarUsuario(Guid.NewGuid()),
            RequestServices = serviceProvider
        };

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var executando = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null);

        var atributo = new RequerPermissaoEscritaPessoaAttribute();
        await atributo.OnActionExecutionAsync(executando, () =>
            Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null)));

        var resultado = executando.Result.Should().BeOfType<ObjectResult>().Subject;
        resultado.StatusCode.Should().Be(400);
    }
}
