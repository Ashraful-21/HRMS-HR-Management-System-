using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using HRMS.Helpers;
using ValidationException = HRMS.Helpers.ValidationException;

namespace HRMS.Middleware;

/// <summary>
/// Global exception handler. Maps domain exceptions → RFC 7807 ProblemDetails.
/// No stack traces in production.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(ctx, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        var (status, title) = ex switch
        {
            NotFoundException       => (HttpStatusCode.NotFound,                  "Not Found"),
            ConflictException       => (HttpStatusCode.Conflict,                  "Conflict"),
            BusinessRuleException   => (HttpStatusCode.UnprocessableEntity,       "Business Rule Violation"),
            ForbiddenException      => (HttpStatusCode.Forbidden,                 "Forbidden"),
            ConcurrencyException    => (HttpStatusCode.Conflict,                  "Concurrency Conflict"),
            ValidationException     => (HttpStatusCode.UnprocessableEntity,       "Validation Failed"),
            UnauthorizedException   => (HttpStatusCode.Unauthorized,              "Unauthorized"),
            _                       => (HttpStatusCode.InternalServerError,        "Internal Server Error")
        };

        var problem = new ProblemDetails
        {
            Status   = (int)status,
            Title    = title,
            Detail   = ex.Message,
            Instance = ctx.Request.Path
        };

        if (ex is ValidationException ve)
            problem.Extensions["errors"] = ve.Errors;

        if (env.IsDevelopment())
            problem.Extensions["stackTrace"] = ex.StackTrace;

        ctx.Response.ContentType = "application/problem+json";
        ctx.Response.StatusCode  = (int)status;

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return ctx.Response.WriteAsync(json);
    }
}
