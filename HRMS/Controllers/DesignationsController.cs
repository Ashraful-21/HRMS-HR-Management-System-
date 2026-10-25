using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Common;
using HRMS.Dtos.Organization;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/designations")]
[Authorize]
[Produces("application/json")]
public class DesignationsController(IOrganizationService orgService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.DesignationsRead)]
    [ProducesResponseType(typeof(ApiResponse<List<DesignationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDesignations([FromQuery] int? departmentId)
    {
        var result = await orgService.GetDesignationsAsync(departmentId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.DesignationsRead)]
    [ProducesResponseType(typeof(ApiResponse<DesignationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DesignationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDesignation(int id)
    {
        var result = await orgService.GetDesignationByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.DesignationsWrite)]
    [ProducesResponseType(typeof(ApiResponse<DesignationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DesignationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDesignation([FromBody] CreateDesignationRequest request)
    {
        var result = await orgService.CreateDesignationAsync(request, currentUser.UserId);
        return result.Success ? CreatedAtAction(nameof(GetDesignation), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.DesignationsWrite)]
    [ProducesResponseType(typeof(ApiResponse<DesignationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DesignationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateDesignation(int id, [FromBody] UpdateDesignationRequest request)
    {
        var result = await orgService.UpdateDesignationAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.DesignationsWrite)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteDesignation(int id)
    {
        var result = await orgService.DeleteDesignationAsync(id, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
