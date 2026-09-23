using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortalFinanceiro.Core.Application.Interfaces;
using PortalFinanceiro.Core.Application.Services;
using PortalFinanceiro.Core.Domain.Interfaces.Repositories;
using PortalFinanceiro.Core.Domain.Interfaces.Services;
using PortalFinanceiro.Infrastructure.Data;
using PortalFinanceiro.Infrastructure.Data.Providers;
using PortalFinanceiro.Infrastructure.Repositories;
using PortalFinanceiro.Infrastructure.Services;
using PortalFinanceiro.Infrastructure.Sql;
using PortalFinanceiro.Infrastructure.Sql.Dialects;

namespace PortalFinanceiro.Infrastructure.IoC;

public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var provider = configuration["Database:Provider"] ?? configuration["Database__Provider"] ?? "SqlServer";
        var isPostgres = provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase)
                      || provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
                      || provider.Equals("Npgsql", StringComparison.OrdinalIgnoreCase);

        if (isPostgres)
        {
            services.AddSingleton<IDatabaseConnectionFactory>(_ => new PostgresConnectionFactory(connectionString));
            SqlDialect.Configure(new PostgresDialect());
        }
        else
        {
            services.AddSingleton<IDatabaseConnectionFactory>(_ => new SqlServerConnectionFactory(connectionString));
            SqlDialect.Configure(new SqlServerDialect());
        }
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
        services.AddScoped<IParceriaRepository, ParceriaRepository>();
        services.AddScoped<IContratoRepository, ContratoRepository>();
        services.AddScoped<IReceitaServicoRepository, ReceitaServicoRepository>();
        services.AddScoped<IDespesaServicoRepository, DespesaServicoRepository>();

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
        services.AddScoped<IParceriaAppService, ParceriaAppService>();
        services.AddScoped<IContratoAppService, ContratoAppService>();
        services.AddScoped<IDiagnosticoAppService, DiagnosticoAppService>();

        return services;
    }
}
