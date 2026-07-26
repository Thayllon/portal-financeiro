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
        services.AddScoped<IContaBancariaRepository, ContaBancariaRepository>();
        services.AddScoped<ICategoriaReceitaRepository, CategoriaReceitaRepository>();
        services.AddScoped<ICategoriaDespesaRepository, CategoriaDespesaRepository>();
        services.AddScoped<IReceitaRecorrenteRepository, ReceitaRecorrenteRepository>();
        services.AddScoped<IDespesaRecorrenteRepository, DespesaRecorrenteRepository>();
        services.AddScoped<IReceitaMensalRepository, ReceitaMensalRepository>();
        services.AddScoped<IDespesaMensalRepository, DespesaMensalRepository>();

        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<IContaBancariaAppService, ContaBancariaAppService>();
        services.AddScoped<ICategoriaReceitaAppService, CategoriaReceitaAppService>();
        services.AddScoped<ICategoriaDespesaAppService, CategoriaDespesaAppService>();
        services.AddScoped<IReceitaRecorrenteAppService, ReceitaRecorrenteAppService>();
        services.AddScoped<IDespesaRecorrenteAppService, DespesaRecorrenteAppService>();
        services.AddScoped<IReceitaMensalAppService, ReceitaMensalAppService>();
        services.AddScoped<IDespesaMensalAppService, DespesaMensalAppService>();
        services.AddScoped<IDashboardAppService, DashboardAppService>();

        return services;
    }
}
