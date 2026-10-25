using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class EmployeeLifecycleEvent
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string EventType { get; set; } = null!;

    public DateOnly EventDate { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
