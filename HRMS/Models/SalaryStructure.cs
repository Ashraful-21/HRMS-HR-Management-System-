using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class SalaryStructure
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public decimal GrossSalary { get; set; }

    public bool IsCurrent { get; set; }

    public string? Remarks { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<PayrollDetail> PayrollDetails { get; set; } = new List<PayrollDetail>();

    public virtual ICollection<SalaryStructureComponent> SalaryStructureComponents { get; set; } = new List<SalaryStructureComponent>();
}
