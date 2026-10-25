using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class Holiday
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly HolidayDate { get; set; }

    public string Type { get; set; } = null!;

    public int? BranchId { get; set; }

    public bool IsRecurring { get; set; }

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Branch? Branch { get; set; }
}
