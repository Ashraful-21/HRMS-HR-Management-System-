using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class JobOpening
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int DepartmentId { get; set; }

    public int? DesignationId { get; set; }

    public int BranchId { get; set; }

    public int Vacancies { get; set; }

    public string EmploymentType { get; set; } = null!;

    public int? ExperienceMinYrs { get; set; }

    public int? ExperienceMaxYrs { get; set; }

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public string? Description { get; set; }

    public string? Requirements { get; set; }

    public DateOnly PostedDate { get; set; }

    public DateOnly? ClosingDate { get; set; }

    public string Status { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual Branch Branch { get; set; } = null!;

    public virtual Department Department { get; set; } = null!;

    public virtual Designation? Designation { get; set; }
}
