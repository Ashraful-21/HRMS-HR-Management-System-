using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class SalaryStructureComponent
{
    public int Id { get; set; }

    public int SalaryStructureId { get; set; }

    public int ComponentId { get; set; }

    public string CalculationType { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? PercentageBase { get; set; }

    public virtual SalaryComponent Component { get; set; } = null!;

    public virtual SalaryStructure SalaryStructure { get; set; } = null!;
}
