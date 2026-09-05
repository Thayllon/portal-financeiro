using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PortalFinanceiro.API.Authorization;
using PortalFinanceiro.Infrastructure.Extensions;

namespace PortalFinanceiro.API.Configurations;

public static class ConfigureAuth
{
    public static IServiceCollection AddAppAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var authSection = configuration.GetSection(AuthOptions.SectionName);
        services.Configure<AuthOptions>(authSection);

        var authOptions = authSection.Get<AuthOptions>() ?? new AuthOptions();

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
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        return services;
    }
}
