using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Common;
using HRMS.Dtos.Organization;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/shifts")]
[Authorize]
[Produces("application/json")]
public class ShiftsController(IOrganizationService orgService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.ShiftsRead)]
    [ProducesResponseType(typeof(ApiResponse<List<ShiftDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShifts()
    {
        var result = await orgService.GetShiftsAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.ShiftsRead)]
    [ProducesResponseType(typeof(ApiResponse<ShiftDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ShiftDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShift(int id)
    {
        var result = await orgService.GetShiftByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.ShiftsWrite)]
    [ProducesResponseType(typeof(ApiResponse<ShiftDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ShiftDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateShift([FromBody] CreateShiftRequest request)
    {
        var result = await orgService.CreateShiftAsync(request, currentUser.UserId);
        return result.Success ? CreatedAtAction(nameof(GetShift), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.ShiftsWrite)]
    [ProducesResponseType(typeof(ApiResponse<ShiftDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ShiftDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateShift(int id, [FromBody] UpdateShiftRequest request)
    {
        var result = await orgService.UpdateShiftAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.ShiftsWrite)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteShift(int id)
    {
        var result = await orgService.DeleteShiftAsync(id, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
