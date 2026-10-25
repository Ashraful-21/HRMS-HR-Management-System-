using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class PayrollRun
{
    public int Id { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public int? BranchId { get; set; }

    public string Status { get; set; } = null!;

    public int WorkingDays { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int? ProcessedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? PaidAt { get; set; }

    public int? PaidBy { get; set; }

    public DateTime? LockedAt { get; set; }

    public int? LockedBy { get; set; }

    public string? Remarks { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<PayrollDetail> PayrollDetails { get; set; } = new List<PayrollDetail>();
}
