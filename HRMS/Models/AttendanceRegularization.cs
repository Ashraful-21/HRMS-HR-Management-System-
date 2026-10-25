using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class AttendanceRegularization
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int? AttendanceLogId { get; set; }

    public DateOnly LogDate { get; set; }

    public DateTime? RequestedCheckIn { get; set; }

    public DateTime? RequestedCheckOut { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewRemarks { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual AttendanceLog? AttendanceLog { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Employee? ReviewedByNavigation { get; set; }
}
