using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class LeaveApplication
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int LeaveTypeId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool IsHalfDay { get; set; }

    public string? HalfDayType { get; set; }

    public decimal TotalDays { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? AttachmentUrl { get; set; }

    public string? AttachmentPublicId { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public int? RejectedBy { get; set; }

    public DateTime? RejectedAt { get; set; }

    public string? ApprovalRemarks { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Employee? ApprovedByNavigation { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual LeaveType LeaveType { get; set; } = null!;

    public virtual Employee? RejectedByNavigation { get; set; }
}
