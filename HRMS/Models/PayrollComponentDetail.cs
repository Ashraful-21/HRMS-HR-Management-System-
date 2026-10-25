using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class PayrollComponentDetail
{
    public int Id { get; set; }

    public int PayrollDetailId { get; set; }

    public int ComponentId { get; set; }

    public decimal Amount { get; set; }

    public virtual SalaryComponent Component { get; set; } = null!;

    public virtual PayrollDetail PayrollDetail { get; set; } = null!;
}
