using Microsoft.AspNetCore.Mvc;

namespace PortalFinanceiro.API.Configurations;

public static class ConfigureVersioning
{
    public static IServiceCollection AddAppVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        return services;
    }
}
