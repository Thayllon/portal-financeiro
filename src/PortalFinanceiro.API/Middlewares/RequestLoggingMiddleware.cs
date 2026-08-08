using System.Diagnostics;
using System.Security.Claims;

namespace PortalFinanceiro.API.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var metodo = context.Request.Method;
        var rota = context.Request.Path.ToString();

        await _next(context);

        sw.Stop();
        var status = context.Response.StatusCode;
        var level = status >= 500 ? LogLevel.Error : status >= 400 ? LogLevel.Warning : LogLevel.Information;

        var usuario = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userLog = usuario is null ? "-" : usuario;

        _logger.Log(level,
            "HTTP {Metodo} {Rota} -> {Status} em {Ms}ms | Usuario: {Usuario}",
            metodo, rota, status, sw.ElapsedMilliseconds, userLog);
    }
}