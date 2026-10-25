using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class Designation
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Code { get; set; } = null!;

    public int DepartmentId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<JobOpening> JobOpenings { get; set; } = new List<JobOpening>();
}
