using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Common;
using HRMS.Dtos.Organization;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/holidays")]
[Authorize]
[Produces("application/json")]
public class HolidaysController(IOrganizationService orgService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.HolidaysRead)]
    [ProducesResponseType(typeof(ApiResponse<List<HolidayDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHolidays([FromQuery] int? year, [FromQuery] int? branchId)
    {
        var result = await orgService.GetHolidaysAsync(year, branchId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.HolidaysRead)]
    [ProducesResponseType(typeof(ApiResponse<HolidayDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HolidayDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHoliday(int id)
    {
        var result = await orgService.GetHolidayByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.HolidaysWrite)]
    [ProducesResponseType(typeof(ApiResponse<HolidayDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<HolidayDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateHoliday([FromBody] CreateHolidayRequest request)
    {
        var result = await orgService.CreateHolidayAsync(request, currentUser.UserId);
        return result.Success ? CreatedAtAction(nameof(GetHoliday), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.HolidaysWrite)]
    [ProducesResponseType(typeof(ApiResponse<HolidayDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HolidayDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateHoliday(int id, [FromBody] UpdateHolidayRequest request)
    {
        var result = await orgService.UpdateHolidayAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.HolidaysWrite)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteHoliday(int id)
    {
        var result = await orgService.DeleteHolidayAsync(id, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
