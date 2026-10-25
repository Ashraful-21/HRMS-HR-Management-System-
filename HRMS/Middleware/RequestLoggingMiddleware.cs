namespace HRMS.Middleware;

/// <summary>
/// Logs every incoming request with method, path, status code, elapsed time,
/// and a correlation ID propagated via X-Correlation-Id header.
/// </summary>
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        // Attach or generate correlation ID
        var correlationId = ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N");
        ctx.Response.Headers.Append("X-Correlation-Id", correlationId);

        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["CorrelationId"] = correlationId,
                   ["RequestPath"]   = ctx.Request.Path.Value ?? ""
               }))
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await next(ctx);
            }
            finally
            {
                sw.Stop();
                logger.LogInformation(
                    "HTTP {Method} {Path} => {StatusCode} in {ElapsedMs}ms  [CID:{CorrelationId}]",
                    ctx.Request.Method,
                    ctx.Request.Path,
                    ctx.Response.StatusCode,
                    sw.ElapsedMilliseconds,
                    correlationId);
            }
        }
    }
}
