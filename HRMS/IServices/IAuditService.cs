namespace HRMS.IServices;

public interface IAuditService
{
    Task LogAsync(string action, string? entityType = null, string? entityId = null,
        object? oldValues = null, object? newValues = null, CancellationToken ct = default);
}
