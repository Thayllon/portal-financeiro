using PortalFinanceiro.API.Configurations;
using PortalFinanceiro.API.Middlewares;
using PortalFinanceiro.Infrastructure.IoC;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog();
builder.Services.AddAppCors(builder.Configuration);
builder.Services.AddAppAuth(builder.Configuration);
builder.Services.AddAppSwagger();
builder.Services.AddAppControllers();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

try
{
    Log.Information("=== Portal Financeiro API iniciando ===");
    Log.Information("Ambiente: {Ambiente}", app.Environment.EnvironmentName);
    Log.Information("Swagger: {Url}", "http://localhost:5178/swagger");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erro fatal ao iniciar aplicação");
}
finally
{
    Log.CloseAndFlush();
}

public static partial class ProgramExtensions
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/portal-financeiro-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}
