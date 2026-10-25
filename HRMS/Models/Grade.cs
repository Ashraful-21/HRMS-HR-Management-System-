using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class Grade
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public decimal? MinSalary { get; set; }

    public decimal? MaxSalary { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
