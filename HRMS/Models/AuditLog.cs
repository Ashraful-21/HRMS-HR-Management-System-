using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class AuditLog
{
    public long Id { get; set; }

    public int? UserId { get; set; }

    public string? Username { get; set; }

    public string Action { get; set; } = null!;

    public string? EntityType { get; set; }

    public string? EntityId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime Timestamp { get; set; }
}
