using PortalFinanceiro.API.Configurations;
using PortalFinanceiro.API.Middlewares;
using PortalFinanceiro.Infrastructure.IoC;
using PortalFinanceiro.Infrastructure.Sql;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog();
builder.Services.AddAppCors();
builder.Services.AddAppAuth(builder.Configuration);
builder.Services.AddAppVersioning();
builder.Services.AddAppSwagger();
builder.Services.AddAppControllers();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

SqlDialect.Configure(new PortalFinanceiro.Infrastructure.Sql.Dialects.SqlServerDialect());

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
    Log.Information("Iniciando Portal Financeiro API");
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
