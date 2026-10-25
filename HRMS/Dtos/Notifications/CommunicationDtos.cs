namespace HRMS.Dtos.Notifications;

public class AnnouncementDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public bool IsPinned { get; set; }
    public DateTime PublishFrom { get; set; }
    public DateTime? PublishTo { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public int? BranchId { get; set; }
    public bool IsPinned { get; set; } = false;
    public DateTime? PublishFrom { get; set; }
    public DateTime? PublishTo { get; set; }
}

public class UpdateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public int? BranchId { get; set; }
    public bool IsPinned { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTime? PublishTo { get; set; }
}

public class NotificationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info"; // Info, Success, Warning, Alert
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateNotificationRequest
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info";
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
}
