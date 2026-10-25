namespace HRMS.Middleware;

/// <summary>
/// Adds security headers to every response (OWASP recommendations).
/// </summary>
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        ctx.Response.Headers.Append("X-Content-Type-Options",    "nosniff");
        ctx.Response.Headers.Append("X-Frame-Options",            "DENY");
        ctx.Response.Headers.Append("X-XSS-Protection",           "1; mode=block");
        ctx.Response.Headers.Append("Referrer-Policy",             "strict-origin-when-cross-origin");
        ctx.Response.Headers.Append("Permissions-Policy",          "camera=(), microphone=(), geolocation=()");
        ctx.Response.Headers.Append(
            "Content-Security-Policy",
            "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:;");

        await next(ctx);
    }
}
