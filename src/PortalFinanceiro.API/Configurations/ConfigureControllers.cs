using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace PortalFinanceiro.API.Configurations;

public static class ConfigureControllers
{
    public static IServiceCollection AddAppControllers(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        return services;
    }
}
