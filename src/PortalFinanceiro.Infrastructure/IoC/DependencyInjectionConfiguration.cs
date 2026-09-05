using Microsoft.Extensions.DependencyInjection;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Application.Services;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Interfaces.Services;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Data.Providers;
using PortalFinanceiro.Infrastructure.Repositories;
using PortalFinanceiro.Infrastructure.Services;

namespace PortalFinanceiro.Infrastructure.IoC;

public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDatabaseConnectionFactory>(_ => new SqlServerConnectionFactory(connectionString));
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IPermissaoUsuarioRepository, PermissaoUsuarioRepository>();
        services.AddScoped<IContaBancariaRepository, ContaBancariaRepository>();
        services.AddScoped<ICategoriaReceitaRepository, CategoriaReceitaRepository>();
        services.AddScoped<ICategoriaDespesaRepository, CategoriaDespesaRepository>();
        services.AddScoped<ICategoriaServicoRepository, CategoriaServicoRepository>();
        services.AddScoped<ICategoriaHistoricoRepository, CategoriaHistoricoRepository>();
        services.AddScoped<IReceitaRepository, ReceitaRepository>();
        services.AddScoped<IDespesaRepository, DespesaRepository>();
        services.AddScoped<IRegraReceitaRepository, RegraReceitaRepository>();
        services.AddScoped<IRegraDespesaRepository, RegraDespesaRepository>();
        services.AddScoped<IPessoaRepository, PessoaRepository>();

        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<IUsuarioAppService, UsuarioAppService>();
        services.AddScoped<IPermissaoUsuarioAppService, PermissaoUsuarioAppService>();
        services.AddScoped<IContaBancariaAppService, ContaBancariaAppService>();
        services.AddScoped<ICategoriaReceitaAppService, CategoriaReceitaAppService>();
        services.AddScoped<ICategoriaDespesaAppService, CategoriaDespesaAppService>();
        services.AddScoped<ICategoriaServicoAppService, CategoriaServicoAppService>();
        services.AddScoped<IReceitaAppService, ReceitaAppService>();
        services.AddScoped<IDespesaAppService, DespesaAppService>();
        services.AddScoped<IRegraReceitaAppService, RegraReceitaAppService>();
        services.AddScoped<IRegraDespesaAppService, RegraDespesaAppService>();
        services.AddScoped<IDashboardAppService, DashboardAppService>();
        services.AddScoped<IPessoaAppService, PessoaAppService>();

        return services;
    }
}
