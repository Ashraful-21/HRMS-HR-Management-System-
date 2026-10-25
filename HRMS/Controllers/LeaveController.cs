using System;
using System.Collections.Generic;
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
[Route("api/v1/leave")]
[Authorize]
[Produces("application/json")]
public class LeaveController(
    ILeaveService leaveService,
    ICurrentUserService currentUser) : ControllerBase
{
    // ==========================================
    // 1. LEAVE TYPES
    // ==========================================
    [HttpGet("types")]
    [ProducesResponseType(typeof(ApiResponse<List<LeaveTypeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveTypes()
    {
        var result = await leaveService.GetLeaveTypesAsync();
        return Ok(result);
    }

    [HttpPost("types")]
    [HasPermission(Permissions.LeaveManage)]
    [ProducesResponseType(typeof(ApiResponse<LeaveTypeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<LeaveTypeDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLeaveType([FromBody] CreateLeaveTypeRequest req)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await leaveService.CreateLeaveTypeAsync(req, userId);
        return result.Success
            ? CreatedAtAction(nameof(GetLeaveTypes), null, result)
            : BadRequest(result);
    }

    [HttpPut("types/{id:int}")]
    [HasPermission(Permissions.LeaveManage)]
    [ProducesResponseType(typeof(ApiResponse<LeaveTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeaveTypeDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateLeaveType([FromRoute] int id, [FromBody] UpdateLeaveTypeRequest req)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await leaveService.UpdateLeaveTypeAsync(id, req, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("types/{id:int}")]
    [HasPermission(Permissions.LeaveManage)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteLeaveType([FromRoute] int id)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await leaveService.DeleteLeaveTypeAsync(id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ==========================================
    // 2. LEAVE BALANCES
    // ==========================================
    [HttpGet("balances")]
    [ProducesResponseType(typeof(ApiResponse<List<LeaveBalanceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveBalances([FromQuery] int? employeeId, [FromQuery] int? year)
    {
        // If employeeId is not specified, use currently authenticated user's employeeId
        var targetEmpId = employeeId ?? currentUser.EmployeeId ?? 1;
        var result = await leaveService.GetEmployeeLeaveBalancesAsync(targetEmpId, year);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("balances/allocate")]
    [HasPermission(Permissions.LeaveManage)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AllocateLeaveBalance([FromBody] AllocateLeaveBalanceRequest req)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await leaveService.AllocateLeaveBalanceAsync(req, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("balances/year-end-carry-forward")]
    [HasPermission(Permissions.LeaveManage)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> YearEndCarryForward([FromBody] YearEndCarryForwardRequest req)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await leaveService.ProcessYearEndCarryForwardAsync(req, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ==========================================
    // 3. LEAVE APPLICATIONS
    // ==========================================
    [HttpPost("applications")]
    [HasPermission(Permissions.LeaveApply)]
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyLeave([FromBody] ApplyLeaveRequest req)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await leaveService.ApplyLeaveAsync(req, userId);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }

    [HttpGet("applications")]
    [HasPermission(Permissions.LeaveRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<LeaveApplicationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveApplications([FromQuery] LeaveApplicationFilterDto filter)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await leaveService.GetLeaveApplicationsAsync(filter, userId);
        return Ok(result);
    }

    [HttpGet("applications/{id:int}")]
    [HasPermission(Permissions.LeaveRead)]
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLeaveApplicationById([FromRoute] int id)
    {
        var result = await leaveService.GetLeaveApplicationByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("applications/{id:int}/review")]
    [HasPermission(Permissions.LeaveApprove)]
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeaveApplicationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReviewLeaveApplication([FromRoute] int id, [FromBody] ReviewLeaveApplicationRequest req)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await leaveService.ReviewLeaveApplicationAsync(id, req, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("applications/{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelLeaveApplication([FromRoute] int id)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await leaveService.CancelLeaveApplicationAsync(id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
