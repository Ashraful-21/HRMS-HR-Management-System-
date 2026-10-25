using HRMS.Auth;
using HRMS.Constants;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
[Produces("application/json")]
public class ReportsController(IDashboardService service) : ControllerBase
{
    [HttpGet("attendance")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetAttendanceReport([FromQuery] int month, [FromQuery] int year, [FromQuery] int? departmentId)
    {
        if (month < 1 || month > 12) month = DateTime.UtcNow.Month;
        if (year < 2000) year = DateTime.UtcNow.Year;

        var result = await service.GetAttendanceReportAsync(month, year, departmentId);
        return Ok(result);
    }

    [HttpGet("leaves")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetLeaveReport([FromQuery] int? year, [FromQuery] int? departmentId)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var result = await service.GetLeaveSummaryReportAsync(targetYear, departmentId);
        return Ok(result);
    }

    [HttpGet("payroll")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetPayrollReport([FromQuery] int month, [FromQuery] int year, [FromQuery] int? branchId)
    {
        if (month < 1 || month > 12) month = DateTime.UtcNow.Month;
        if (year < 2000) year = DateTime.UtcNow.Year;

        var result = await service.GetPayrollSummaryReportAsync(month, year, branchId);
        return Ok(result);
    }
}
