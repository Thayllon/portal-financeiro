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

app.SeedDatabase();

try
{
    var conexao = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
    Log.Information("=== Portal Financeiro API iniciando ===");
    Log.Information("Ambiente: {Ambiente} | Conexao: {Conexao}", app.Environment.EnvironmentName, conexao);
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

    public static void SeedDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var usuarioRepository = scope.ServiceProvider.GetRequiredService<PortalFinanceiro.Core.Domain.Interfaces.Repositories.IUsuarioRepository>();
        var passwordService = scope.ServiceProvider.GetRequiredService<PortalFinanceiro.Core.Domain.Interfaces.Services.IPasswordService>();

        var usuarios = usuarioRepository.ListarAsync().GetAwaiter().GetResult();
        if (usuarios.Any()) return;

        var senhaHash = passwordService.Hash("123456");
        var usuarioResult = PortalFinanceiro.Core.Domain.Entities.Usuario.Criar("Admin", "admin@portal.com", senhaHash);

        if (usuarioResult.EhSucesso)
            usuarioRepository.InserirAsync(usuarioResult.Dado!).GetAwaiter().GetResult();
    }
}
