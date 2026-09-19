namespace PortalFinanceiro.API.Configurations;

public static class ConfigureCors
{
    public static IServiceCollection AddAppCors(this IServiceCollection services, IConfiguration? configuration = null)
    {
        var originsValue = configuration?["Cors:AllowedOrigins"] ?? configuration?["Cors__AllowedOrigins"];
        var origins = string.IsNullOrWhiteSpace(originsValue)
            ? new[] { "http://localhost:4200" }
            : originsValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngular", policy =>
            {
                policy.WithOrigins(origins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        return services;
    }
}
