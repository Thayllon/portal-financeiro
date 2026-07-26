namespace PortalFinanceiro.API.Configurations;

public static class ConfigureCors
{
    public static IServiceCollection AddAppCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngular", policy =>
            {
                policy.SetIsOriginAllowed(origin =>
                    {
                        if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                            return uri.Host == "localhost";
                        return false;
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}
