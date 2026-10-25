using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class AssetAssignment
{
    public int Id { get; set; }

    public int AssetId { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly AssignedDate { get; set; }

    public DateOnly? ReturnedDate { get; set; }

    public string Condition { get; set; } = null!;

    public string? ReturnCondition { get; set; }

    public string? Notes { get; set; }

    public bool IsReturned { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Asset Asset { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;
}
