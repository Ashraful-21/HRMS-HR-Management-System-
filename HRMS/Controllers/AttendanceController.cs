using System;
using System.Threading.Tasks;
using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Common;
using HRMS.DTOs;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/attendance")]
[Authorize]
[Produces("application/json")]
public class AttendanceController(
    IAttendanceService attendanceService,
    ICurrentUserService currentUser) : ControllerBase
{
    private string GetClientIp()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        if (HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var f = forwarded.ToString();
            if (!string.IsNullOrWhiteSpace(f))
                ip = f.Split(',')[0].Trim();
        }
        return ip ?? "127.0.0.1";
    }

    [HttpPost("check-in")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceLogResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceLogResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest req)
    {
        var clientIp = GetClientIp();
        var userId = currentUser.UserId ?? 0;
        var result = await attendanceService.CheckInAsync(req, clientIp, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("check-out")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceLogResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceLogResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest req)
    {
        var clientIp = GetClientIp();
        var userId = currentUser.UserId ?? 0;
        var result = await attendanceService.CheckOutAsync(req, clientIp, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("today")]
    [ProducesResponseType(typeof(ApiResponse<TodayAttendanceStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayStatus()
    {
        var userId = currentUser.UserId ?? 0;
        var result = await attendanceService.GetTodayStatusAsync(userId);
        return Ok(result);
    }

    [HttpPost("manual")]
    [HasPermission(Permissions.AttendanceWrite)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceLogResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceLogResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ManualAttendance([FromBody] ManualAttendanceRequest req)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await attendanceService.ManualAttendanceAsync(req, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [HasPermission(Permissions.AttendanceRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AttendanceLogResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendanceLogs([FromQuery] AttendanceFilterDto filter)
    {
        var result = await attendanceService.GetAttendanceLogsAsync(filter);
        return Ok(result);
    }

    [HttpGet("summary")]
    [HasPermission(Permissions.AttendanceRead)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceMonthlySummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlySummary([FromQuery] int employeeId, [FromQuery] int year, [FromQuery] int month)
    {
        if (year <= 0) year = DateTime.UtcNow.Year;
        if (month <= 0 || month > 12) month = DateTime.UtcNow.Month;
        var result = await attendanceService.GetMonthlySummaryAsync(employeeId, year, month);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("regularize")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceRegularizationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceRegularizationResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestRegularization([FromBody] RegularizationRequestDto req)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await attendanceService.RequestRegularizationAsync(req, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("regularize")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AttendanceRegularizationResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegularizationRequests([FromQuery] int? employeeId, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await attendanceService.GetRegularizationRequestsAsync(employeeId, status, page, pageSize);
        return Ok(result);
    }

    [HttpPost("regularize/{id:int}/review")]
    [HasPermission(Permissions.AttendanceWrite)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceRegularizationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceRegularizationResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReviewRegularization([FromRoute] int id, [FromBody] RegularizationReviewDto req)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await attendanceService.ReviewRegularizationAsync(id, req, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("jobs/run-daily-aggregation")]
    [HasPermission(Permissions.AttendanceWrite)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> TriggerDailyAggregation([FromQuery] DateOnly? date)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var result = await attendanceService.RunDailyAttendanceAggregationAsync(targetDate);
        return Ok(result);
    }
}
