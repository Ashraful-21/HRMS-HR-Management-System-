using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class EmployeeExperience
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string Company { get; set; } = null!;

    public string? Designation { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsCurrent { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
