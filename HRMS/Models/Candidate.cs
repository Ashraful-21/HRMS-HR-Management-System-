using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class Candidate
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? ResumeUrl { get; set; }

    public string? ResumePublicId { get; set; }

    public string? LinkedInUrl { get; set; }

    public string? Source { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
}
