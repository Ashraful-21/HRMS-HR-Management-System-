namespace HRMS.Dtos.Recruitment;

public class JobOpeningDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int? DesignationId { get; set; }
    public string? DesignationTitle { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int Vacancies { get; set; }
    public string EmploymentType { get; set; } = "Full-Time";
    public int? ExperienceMinYrs { get; set; }
    public int? ExperienceMaxYrs { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public DateOnly PostedDate { get; set; }
    public DateOnly? ClosingDate { get; set; }
    public string Status { get; set; } = "Open";
    public int ApplicationsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateJobOpeningRequest
{
    public string Title { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public int BranchId { get; set; }
    public int Vacancies { get; set; } = 1;
    public string EmploymentType { get; set; } = "Full-Time";
    public int? ExperienceMinYrs { get; set; }
    public int? ExperienceMaxYrs { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public DateOnly? ClosingDate { get; set; }
}

public class UpdateJobOpeningRequest
{
    public string Title { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public int BranchId { get; set; }
    public int Vacancies { get; set; }
    public string EmploymentType { get; set; } = "Full-Time";
    public int? ExperienceMinYrs { get; set; }
    public int? ExperienceMaxYrs { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public DateOnly? ClosingDate { get; set; }
    public string Status { get; set; } = "Open";
}

public class CandidateDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ResumeUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Source { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ApplicationsCount { get; set; }
}

public class CreateCandidateRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ResumeUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Source { get; set; } = "Direct";
}

public class ApplicationDto
{
    public int Id { get; set; }
    public int JobOpeningId { get; set; }
    public string JobOpeningTitle { get; set; } = string.Empty;
    public int CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string? CandidatePhone { get; set; }
    public string? ResumeUrl { get; set; }
    public string Stage { get; set; } = "Applied"; // Applied, Shortlisted, Interview, Offered, Hired, Rejected
    public int StageOrder { get; set; } = 1;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int InterviewsCount { get; set; }
}

public class CreateApplicationRequest
{
    public int JobOpeningId { get; set; }
    public int CandidateId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateApplicationStageRequest
{
    public string Stage { get; set; } = "Applied";
    public string? Notes { get; set; }
}

public class InterviewDto
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public int InterviewerId { get; set; }
    public string InterviewerName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Mode { get; set; } = "InPerson"; // InPerson, Online, Phone
    public string? Location { get; set; }
    public string? MeetingLink { get; set; }
    public string? Feedback { get; set; }
    public decimal? Score { get; set; }
    public string? Result { get; set; } // Passed, Failed, Rescheduled
    public DateTime CreatedAt { get; set; }
}

public class ScheduleInterviewRequest
{
    public int ApplicationId { get; set; }
    public int InterviewerId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string Mode { get; set; } = "Online";
    public string? Location { get; set; }
    public string? MeetingLink { get; set; }
}

public class SubmitInterviewFeedbackRequest
{
    public string Feedback { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Result { get; set; } = "Passed";
}

public class OfferLetterDto
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public decimal OfferedSalary { get; set; }
    public DateOnly JoiningDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public string? FileUrl { get; set; }
    public string Status { get; set; } = "Sent"; // Sent, Accepted, Declined, Revoked
    public DateTime CreatedAt { get; set; }
}

public class CreateOfferLetterRequest
{
    public int ApplicationId { get; set; }
    public decimal OfferedSalary { get; set; }
    public DateOnly JoiningDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public string? FileUrl { get; set; }
}

public class UpdateOfferStatusRequest
{
    public string Status { get; set; } = "Accepted"; // Accepted | Declined | Revoked
}
