using PortalFinanceiro.API.Configurations;
using PortalFinanceiro.API.Middlewares;
using PortalFinanceiro.API.Seeders;
using PortalFinanceiro.Infrastructure.Extensions;
using PortalFinanceiro.Infrastructure.IoC;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog();
builder.Services.AddAppCors(builder.Configuration);
builder.Services.AddAppAuth(builder.Configuration);
builder.Services.AddAppSwagger();
builder.Services.AddAppControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection(SeedOptions.SectionName));
builder.Services.AddScoped<DatabaseSeeder>();

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

await InvokeSeedAsync(app);

try
{
    Log.Information("=== Portal Financeiro API iniciando ===");
    Log.Information("Ambiente: {Ambiente}", app.Environment.EnvironmentName);
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erro fatal ao iniciar aplicação");
}
finally
{
    Log.CloseAndFlush();
}

static async Task InvokeSeedAsync(WebApplication app)
{
    try
    {
        using var scope = app.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Erro ao executar seed do banco de dados");
        throw;
    }
}

public static partial class ProgramExtensions
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration.GetSection("Serilog");

        if (!config.GetChildren().Any())
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
        }
        else
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        builder.Host.UseSerilog();
    }
}
