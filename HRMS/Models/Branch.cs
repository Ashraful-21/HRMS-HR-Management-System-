using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class Branch
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public bool IsHeadOffice { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Holiday> Holidays { get; set; } = new List<Holiday>();

    public virtual ICollection<JobOpening> JobOpenings { get; set; } = new List<JobOpening>();

    public virtual ICollection<PayrollRun> PayrollRuns { get; set; } = new List<PayrollRun>();
}
