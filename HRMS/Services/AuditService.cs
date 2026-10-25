using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using HRMS.Data;
using HRMS.IServices;

namespace HRMS.Services;

/// <summary>
/// Writes audit records directly to dbo.AuditLogs via raw SQL to avoid
/// circular SaveChanges calls and keep audit writes fast.
/// After scaffolding, replace the raw SQL with an EF DbSet call.
/// </summary>
public class AuditService(HrmsDbContext db, ICurrentUserService currentUser,
    IHttpContextAccessor httpContext) : IAuditService
{
    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReferenceHandler     = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
        WriteIndented        = false
    };

    public async Task LogAsync(string action, string? entityType = null, string? entityId = null,
        object? oldValues = null, object? newValues = null, CancellationToken ct = default)
    {
        var userId   = currentUser.UserId;
        var username = currentUser.Username;
        var ip       = httpContext.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var ua       = httpContext.HttpContext?.Request.Headers.UserAgent.ToString();
        var oldJson  = oldValues is null ? null : JsonSerializer.Serialize(oldValues, _jsonOpts);
        var newJson  = newValues is null ? null : JsonSerializer.Serialize(newValues, _jsonOpts);

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO dbo.AuditLogs (UserId, Username, Action, EntityType, EntityId, OldValues, NewValues, IpAddress, UserAgent, Timestamp)
            VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, SYSUTCDATETIME())
            """,
            new object?[] { userId, username, action, entityType, entityId, oldJson, newJson, ip, ua },
            ct);
    }
}
