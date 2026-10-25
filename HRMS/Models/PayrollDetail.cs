using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class PayrollDetail
{
    public int Id { get; set; }

    public int PayrollRunId { get; set; }

    public int EmployeeId { get; set; }

    public int SalaryStructureId { get; set; }

    public int WorkingDays { get; set; }

    public decimal PresentDays { get; set; }

    public decimal PaidLeaveDays { get; set; }

    public decimal UnpaidLeaveDays { get; set; }

    public int OvertimeMinutes { get; set; }

    public decimal GrossEarnings { get; set; }

    public decimal TotalDeductions { get; set; }

    public decimal NetPay { get; set; }

    public decimal LossOfPay { get; set; }

    public bool IsEmailSent { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<LoanRepayment> LoanRepayments { get; set; } = new List<LoanRepayment>();

    public virtual ICollection<PayrollComponentDetail> PayrollComponentDetails { get; set; } = new List<PayrollComponentDetail>();

    public virtual PayrollRun PayrollRun { get; set; } = null!;

    public virtual SalaryStructure SalaryStructure { get; set; } = null!;
}
