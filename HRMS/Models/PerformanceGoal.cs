using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class PerformanceGoal
{
    public int Id { get; set; }

    public int ReviewId { get; set; }

    public string GoalDescription { get; set; } = null!;

    public string? Target { get; set; }

    public string? Achievement { get; set; }

    public decimal? Weight { get; set; }

    public decimal? SelfScore { get; set; }

    public decimal? ManagerScore { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PerformanceReview Review { get; set; } = null!;
}
