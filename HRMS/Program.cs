using HRMS.Extensions;
using QuestPDF.Infrastructure;
using Serilog;

// ── Configure Serilog from appsettings.json before anything else ──────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting HRMS API...");

    // QuestPDF Community license
    QuestPDF.Settings.License = LicenseType.Community;

    var builder = WebApplication.CreateBuilder(args);

    // Serilog integration
    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services));

    // Controllers
    builder.Services.AddControllers()
        .AddJsonOptions(opt =>
        {
            opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    // All HRMS services (DI, DB, Auth, etc.)
    builder.Services.AddHrmsServices(builder.Configuration);

    var app = builder.Build();

    // Full pipeline
    app.UseHrmsPipeline();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "HRMS API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
