using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class Department
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public int BranchId { get; set; }

    public int? ParentId { get; set; }

    public int? HeadEmployeeId { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<Designation> Designations { get; set; } = new List<Designation>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual Employee? HeadEmployee { get; set; }

    public virtual ICollection<Department> InverseParent { get; set; } = new List<Department>();

    public virtual ICollection<JobOpening> JobOpenings { get; set; } = new List<JobOpening>();

    public virtual Department? Parent { get; set; }
}
