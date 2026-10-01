namespace PortalFinanceiro.API.Middlewares;

/// <summary>
/// Propaga X-Correlation-Id (ou gera um) para amarrar todos os logs de uma requisição.
/// Deve rodar antes do RequestLoggingMiddleware.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var valores) &&
                            !string.IsNullOrWhiteSpace(valores.ToString())
            ? valores.ToString()
            : Guid.NewGuid().ToString("N");

        context.Response.Headers[HeaderName] = correlationId;

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
