using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class PerformanceReview
{
    public int Id { get; set; }

    public int ReviewCycleId { get; set; }

    public int EmployeeId { get; set; }

    public int ReviewerId { get; set; }

    public string Status { get; set; } = null!;

    public decimal? SelfRating { get; set; }

    public decimal? ManagerRating { get; set; }

    public decimal? FinalRating { get; set; }

    public string? SelfComments { get; set; }

    public string? ManagerComments { get; set; }

    public DateTime? SelfSubmittedAt { get; set; }

    public DateTime? ManagerSubmittedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<PerformanceGoal> PerformanceGoals { get; set; } = new List<PerformanceGoal>();

    public virtual ReviewCycle ReviewCycle { get; set; } = null!;

    public virtual Employee Reviewer { get; set; } = null!;
}
