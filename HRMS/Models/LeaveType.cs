using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class LeaveType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public decimal DaysAllowed { get; set; }

    public bool IsCarryForwardAllowed { get; set; }

    public decimal? MaxCarryForwardDays { get; set; }

    public bool IsPaidLeave { get; set; }

    public bool IsHalfDayAllowed { get; set; }

    public bool IsAttachmentRequired { get; set; }

    public int? AttachmentRequiredAfterDays { get; set; }

    public string? GenderRestriction { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<LeaveApplication> LeaveApplications { get; set; } = new List<LeaveApplication>();

    public virtual ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
}
