using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class VwLeaveBalanceSummary
{
    public int EmployeeId { get; set; }

    public string FullName { get; set; } = null!;

    public string EmployeeCode { get; set; } = null!;

    public string LeaveType { get; set; } = null!;

    public string LeaveTypeCode { get; set; } = null!;

    public int Year { get; set; }

    public decimal TotalDays { get; set; }

    public decimal CarryForwardDays { get; set; }

    public decimal UsedDays { get; set; }

    public decimal? Balance { get; set; }
}
