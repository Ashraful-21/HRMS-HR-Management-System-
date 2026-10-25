using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.Dtos.Common;
using HRMS.DTOs;

namespace HRMS.IServices;

public interface IPayrollService
{
    // Salary Components
    Task<ApiResponse<List<SalaryComponentDto>>> GetSalaryComponentsAsync();
    Task<ApiResponse<SalaryComponentDto>> CreateSalaryComponentAsync(CreateSalaryComponentRequest req, int currentUserId);

    // Salary Structure
    Task<ApiResponse<SalaryStructureDto>> GetEmployeeSalaryStructureAsync(int employeeId);
    Task<ApiResponse<SalaryStructureDto>> AssignSalaryStructureAsync(AssignSalaryStructureRequest req, int currentUserId);

    // Tax Engine
    ApiResponse<TaxCalculationResultDto> CalculateTax(decimal monthlyGross);

    // Payroll Processing Batch
    Task<ApiResponse<PayrollRunSummaryDto>> ProcessPayrollAsync(ProcessPayrollRequest req, int currentUserId);
    Task<ApiResponse<List<PayrollRunSummaryDto>>> GetPayrollRunsAsync(int? year, int? month);
    Task<ApiResponse<PayrollRunSummaryDto>> GetPayrollRunByIdAsync(int id);
    Task<ApiResponse<PayrollRunSummaryDto>> ApprovePayrollRunAsync(int id, int currentUserId);
    Task<ApiResponse<PayrollRunSummaryDto>> MarkPayrollRunAsPaidAsync(int id, int currentUserId);

    // Payslips
    Task<byte[]> GeneratePayslipPdfAsync(int payrollDetailId);
    Task<ApiResponse<List<PayrollDetailDto>>> GetEmployeePayslipsAsync(int employeeId, int? year);
}
