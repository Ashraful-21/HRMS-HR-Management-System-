namespace HRMS.Dtos.Performance;

public class ReviewCycleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Active, Completed, Closed
    public int TotalReviews { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewCycleRequest
{
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; } = DateTime.UtcNow.Year;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

public class UpdateReviewCycleRequest
{
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = "Active";
}

public class PerformanceGoalDto
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public string GoalDescription { get; set; } = string.Empty;
    public string? Target { get; set; }
    public string? Achievement { get; set; }
    public decimal? Weight { get; set; }
    public decimal? SelfScore { get; set; }
    public decimal? ManagerScore { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AddGoalRequest
{
    public string GoalDescription { get; set; } = string.Empty;
    public string? Target { get; set; }
    public string? Achievement { get; set; }
    public decimal? Weight { get; set; }
    public decimal? SelfScore { get; set; }
}

public class PerformanceReviewDto
{
    public int Id { get; set; }
    public int ReviewCycleId { get; set; }
    public string ReviewCycleName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string DesignationTitle { get; set; } = string.Empty;
    public int ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, SelfSubmitted, ManagerReviewed, Finalized
    public decimal? SelfRating { get; set; }
    public decimal? ManagerRating { get; set; }
    public decimal? FinalRating { get; set; }
    public string? SelfComments { get; set; }
    public string? ManagerComments { get; set; }
    public DateTime? SelfSubmittedAt { get; set; }
    public DateTime? ManagerSubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PerformanceGoalDto> Goals { get; set; } = [];
}

public class CreatePerformanceReviewRequest
{
    public int ReviewCycleId { get; set; }
    public int EmployeeId { get; set; }
    public int ReviewerId { get; set; }
}

public class SubmitSelfReviewRequest
{
    public decimal SelfRating { get; set; }
    public string SelfComments { get; set; } = string.Empty;
    public List<AddGoalRequest>? Goals { get; set; }
}

public class SubmitManagerReviewRequest
{
    public decimal ManagerRating { get; set; }
    public string ManagerComments { get; set; } = string.Empty;
    public decimal FinalRating { get; set; }
    public List<GoalScoreUpdate>? GoalScores { get; set; }
}

public class GoalScoreUpdate
{
    public int GoalId { get; set; }
    public decimal ManagerScore { get; set; }
}
