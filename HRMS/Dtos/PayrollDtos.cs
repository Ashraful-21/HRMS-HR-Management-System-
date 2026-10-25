using System;
using System.Collections.Generic;

namespace HRMS.DTOs;

public class SalaryComponentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Earning" | "Deduction"
    public bool IsFixed { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

public class CreateSalaryComponentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = "Earning";
    public bool IsFixed { get; set; }
    public bool IsTaxable { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 1;
}

public class SalaryStructureComponentItem
{
    public int ComponentId { get; set; }
    public string CalculationType { get; set; } = "Percentage"; // "Fixed" | "Percentage"
    public decimal Amount { get; set; } // e.g. 50 for 50%, or 20000 for fixed
    public string? PercentageBase { get; set; } = "Basic"; // "Basic" | "Gross"
}

public class AssignSalaryStructureRequest
{
    public int EmployeeId { get; set; }
    public decimal GrossSalary { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public string? Remarks { get; set; }
    public List<SalaryStructureComponentItem> Components { get; set; } = [];
}

public class SalaryStructureComponentDto
{
    public int Id { get; set; }
    public int ComponentId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public string ComponentCode { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public string CalculationType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? PercentageBase { get; set; }
    public decimal CalculatedMonthlyAmount { get; set; }
}

public class SalaryStructureDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public decimal GrossSalary { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsCurrent { get; set; }
    public string? Remarks { get; set; }
    public List<SalaryStructureComponentDto> Components { get; set; } = [];
}

public class ProcessPayrollRequest
{
    public int Month { get; set; }
    public int Year { get; set; }
    public int? BranchId { get; set; }
    public int WorkingDays { get; set; } = 22;
    public string? Remarks { get; set; }
}

public class PayrollComponentDetailDto
{
    public int ComponentId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public string ComponentCode { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class PayrollDetailDto
{
    public int Id { get; set; }
    public int PayrollRunId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? DesignationTitle { get; set; }
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public int WorkingDays { get; set; }
    public decimal PresentDays { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
    public int OvertimeMinutes { get; set; }
    public decimal GrossEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
    public decimal LossOfPay { get; set; }
    public List<PayrollComponentDetailDto> Components { get; set; } = [];
}

public class PayrollRunSummaryDto
{
    public int Id { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string Status { get; set; } = "Draft"; // "Draft", "Approved", "Paid"
    public int WorkingDays { get; set; }
    public int TotalEmployees { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetPay { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Remarks { get; set; }
    public List<PayrollDetailDto> Details { get; set; } = [];
}

public class TaxCalculationResultDto
{
    public decimal MonthlyGross { get; set; }
    public decimal AnnualGross { get; set; }
    public decimal TaxExemptAmount { get; set; }
    public decimal TaxableIncome { get; set; }
    public decimal EstimatedAnnualTax { get; set; }
    public decimal MonthlyTds { get; set; }
    public decimal EffectiveTaxRatePercentage { get; set; }
}
