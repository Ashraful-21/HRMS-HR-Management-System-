using AspNetCoreRateLimit;
using Hangfire;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using HRMS.Data;
using HRMS.Hubs;
using HRMS.IServices;
using HRMS.Middleware;

namespace HRMS.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>Master pipeline configuration — called once from Program.cs.</summary>
    public static WebApplication UseHrmsPipeline(this WebApplication app)
    {
        // Security & logging
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Rate limiting
        app.UseIpRateLimiting();

        // Response compression
        app.UseResponseCompression();

        // OpenAPI + Scalar (available in all environments for this build)
        app.MapOpenApi();
        app.MapScalarApiReference(opt =>
        {
            opt.Title = "HRMS API";
            opt.Theme = ScalarTheme.DeepSpace;
            opt.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        // CORS (before Auth)
        app.UseCors("HrmsCors");

        // HTTPS redirect
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        // Auth pipeline
        app.UseAuthentication();
        app.UseAuthorization();

        // Health check endpoint (no auth required)
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteAsync
        });

        // Static files (uploads/documents/photos)
        app.UseStaticFiles();

        // Controllers
        app.MapControllers();

        // SignalR hubs
        app.MapHub<NotificationHub>("/hubs/notifications");

        // Hangfire dashboard (SuperAdmin only)
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [new HangfireDashboardAuthFilter()]
        });

        // Recurring Hangfire Jobs: Daily attendance aggregation (2:00 AM UTC)
        RecurringJob.AddOrUpdate<IAttendanceService>(
            "daily-attendance-aggregation",
            service => service.RunDailyAttendanceAggregationAsync(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))),
            "0 2 * * *");

        // Seed database on startup
        app.SeedDatabaseAsync().GetAwaiter().GetResult();

        return app;
    }

    private static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync();
    }
}
