using HRMS.Dtos.Common;
using HRMS.Dtos.Dashboard;
using HRMS.Dtos.Reports;

namespace HRMS.IServices;

public interface IDashboardService
{
    Task<ApiResponse<DashboardSummaryDto>> GetDashboardSummaryAsync();
    Task<ApiResponse<List<AttendanceReportItemDto>>> GetAttendanceReportAsync(int month, int year, int? departmentId = null);
    Task<ApiResponse<List<LeaveReportItemDto>>> GetLeaveSummaryReportAsync(int year, int? departmentId = null);
    Task<ApiResponse<List<PayrollReportItemDto>>> GetPayrollSummaryReportAsync(int month, int year, int? branchId = null);
}
