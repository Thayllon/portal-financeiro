using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PortalFinanceiro.Infrastructure.Extensions;

namespace PortalFinanceiro.API.Configurations;

public static class ConfigureAuth
{
    public static IServiceCollection AddAppAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var authSection = configuration.GetSection(AuthOptions.SectionName);
        services.Configure<AuthOptions>(authSection);

        var authOptions = authSection.Get<AuthOptions>() ?? new AuthOptions();

        if (string.IsNullOrWhiteSpace(authOptions.Secret))
            throw new InvalidOperationException(
                "A chave secreta JWT (Auth__Secret) não foi configurada. Defina a variável de ambiente Auth__Secret antes de iniciar a aplicação.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authOptions.Issuer,
                ValidAudience = authOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Secret))
            };
        });

        services.AddAuthorization();

        return services;
    }
}
