using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class EmployeeEducation
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string Degree { get; set; } = null!;

    public string Institution { get; set; } = null!;

    public string? FieldOfStudy { get; set; }

    public int? StartYear { get; set; }

    public int? EndYear { get; set; }

    public string? Grade { get; set; }

    public bool IsHighest { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
