using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class LoanRepayment
{
    public int Id { get; set; }

    public int LoanId { get; set; }

    public int? PayrollDetailId { get; set; }

    public decimal Amount { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Loan Loan { get; set; } = null!;

    public virtual PayrollDetail? PayrollDetail { get; set; }
}
