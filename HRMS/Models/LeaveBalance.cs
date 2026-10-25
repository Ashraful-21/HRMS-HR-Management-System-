using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class LeaveBalance
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int LeaveTypeId { get; set; }

    public int Year { get; set; }

    public decimal TotalDays { get; set; }

    public decimal UsedDays { get; set; }

    public decimal CarryForwardDays { get; set; }

    public decimal? Balance { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual LeaveType LeaveType { get; set; } = null!;
}
