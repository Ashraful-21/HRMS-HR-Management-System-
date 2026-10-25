using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class Interview
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public int InterviewerId { get; set; }

    public DateTime ScheduledAt { get; set; }

    public string Mode { get; set; } = null!;

    public string? Location { get; set; }

    public string? MeetingLink { get; set; }

    public string? Feedback { get; set; }

    public decimal? Score { get; set; }

    public string? Result { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Application Application { get; set; } = null!;

    public virtual Employee Interviewer { get; set; } = null!;
}
