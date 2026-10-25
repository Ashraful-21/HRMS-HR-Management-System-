using System.Text;
using AspNetCoreRateLimit;
using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using HRMS.Auth;
using HRMS.Data;
using HRMS.IRepositories;
using HRMS.IServices;
using HRMS.Options;
using HRMS.Repositories;
using HRMS.Services;

namespace HRMS.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>Master registration — called once from Program.cs.</summary>
    public static IServiceCollection AddHrmsServices(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddOptions(config)
            .AddDatabase(config)
            .AddJwtAuth(config)
            .AddPermissionAuthorization()
            .AddRepositories()
            .AddDistributedCacheConfig(config)
            .AddCoreServices()
            .AddValidators()
            .AddHangfireJobs(config)
            .AddSignalRHub()
            .AddRateLimiting(config)
            .AddResponseCompressionGzip()
            .AddOpenApiDocs()
            .AddCorsPolicy(config)
            .AddDbHealthChecks(config);

        return services;
    }

    private static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtSettings>(config.GetSection(JwtSettings.SectionName));
        services.Configure<SeedOptions>(config.GetSection(SeedOptions.SectionName));
        services.Configure<CloudinaryOptions>(config.GetSection(CloudinaryOptions.SectionName));
        services.Configure<SmtpOptions>(config.GetSection(SmtpOptions.SectionName));
        services.Configure<CorsOptions>(config.GetSection(CorsOptions.SectionName));
        services.Configure<RedisOptions>(config.GetSection(RedisOptions.SectionName));
        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<HrmsDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("Default"),
                sql => sql.EnableRetryOnFailure(3)));

        services.AddScoped<DbSeeder>();
        return services;
    }

    private static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = config.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                          ?? throw new InvalidOperationException("JWT settings are not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = jwtSettings.Issuer,
                    ValidAudience            = jwtSettings.Audience,
                    IssuerSigningKey         = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                // Support SignalR: read token from query string for hub connections
                opt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var token = ctx.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(token) &&
                            ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                            ctx.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    private static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        // Feature repositories registered here as they are created in Phase 3+
        return services;
    }

    private static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ILeaveService, LeaveService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<IRecruitmentService, RecruitmentService>();
        services.AddScoped<IPerformanceService, PerformanceService>();
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<ICommunicationService, CommunicationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IEmailService, EmailService>();
        return services;
    }

    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Scoped);
        return services;
    }

    private static IServiceCollection AddHangfireJobs(this IServiceCollection services, IConfiguration config)
    {
        var connStr = config.GetConnectionString("Default");
        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connStr, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout      = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval           = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks           = true
            }));
        services.AddHangfireServer();
        return services;
    }

    private static IServiceCollection AddSignalRHub(this IServiceCollection services)
    {
        services.AddSignalR();
        return services;
    }

    private static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration config)
    {
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(config.GetSection("IpRateLimiting"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        return services;
    }

    private static IServiceCollection AddResponseCompressionGzip(this IServiceCollection services)
    {
        services.AddResponseCompression(opt => opt.EnableForHttps = true);
        return services;
    }

    private static IServiceCollection AddOpenApiDocs(this IServiceCollection services)
    {
        services.AddOpenApi(opt =>
        {
            opt.AddDocumentTransformer((doc, ctx, ct) =>
            {
                doc.Info = new()
                {
                    Title       = "HRMS API",
                    Version     = "v1",
                    Description = "Production-grade Human Resource Management System API"
                };
                return Task.CompletedTask;
            });
        });

        return services;
    }

    private static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration config)
    {
        var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(opt => opt.AddPolicy("HrmsCors", policy =>
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()));

        return services;
    }

    private static IServiceCollection AddDbHealthChecks(this IServiceCollection services, IConfiguration config)
    {
        services.AddHealthChecks()
            .AddSqlServer(
                config.GetConnectionString("Default") ?? string.Empty,
                name: "database",
                tags: ["db", "sql"]);
        return services;
    }

    private static IServiceCollection AddDistributedCacheConfig(this IServiceCollection services, IConfiguration config)
    {
        var redis = config.GetSection(RedisOptions.SectionName).Get<RedisOptions>() ?? new RedisOptions();
        if (redis.Enabled && !string.IsNullOrWhiteSpace(redis.ConnectionString))
        {
            services.AddStackExchangeRedisCache(opt =>
            {
                opt.Configuration = redis.ConnectionString;
                opt.InstanceName = redis.InstanceName;
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton<ICacheService, CacheService>();
        return services;
    }
}

