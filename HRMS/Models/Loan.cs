using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class Loan
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string LoanType { get; set; } = null!;

    public decimal Amount { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal? OutstandingAmount { get; set; }

    public int Installments { get; set; }

    public decimal InstallmentAmount { get; set; }

    public int? StartMonth { get; set; }

    public int? StartYear { get; set; }

    public string Status { get; set; } = null!;

    public string? Reason { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Employee? ApprovedByNavigation { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<LoanRepayment> LoanRepayments { get; set; } = new List<LoanRepayment>();
}
