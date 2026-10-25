using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class SalaryComponent
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Type { get; set; } = null!;

    public bool IsFixed { get; set; }

    public bool IsTaxable { get; set; }

    public bool IsActive { get; set; }

    public int SortOrder { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<PayrollComponentDetail> PayrollComponentDetails { get; set; } = new List<PayrollComponentDetail>();

    public virtual ICollection<SalaryStructureComponent> SalaryStructureComponents { get; set; } = new List<SalaryStructureComponent>();
}
