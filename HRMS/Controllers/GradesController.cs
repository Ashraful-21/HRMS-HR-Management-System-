using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Common;
using HRMS.Dtos.Organization;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/grades")]
[Authorize]
[Produces("application/json")]
public class GradesController(IOrganizationService orgService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.GradesRead)]
    [ProducesResponseType(typeof(ApiResponse<List<GradeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGrades()
    {
        var result = await orgService.GetGradesAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.GradesRead)]
    [ProducesResponseType(typeof(ApiResponse<GradeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GradeDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGrade(int id)
    {
        var result = await orgService.GetGradeByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.GradesWrite)]
    [ProducesResponseType(typeof(ApiResponse<GradeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<GradeDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGrade([FromBody] CreateGradeRequest request)
    {
        var result = await orgService.CreateGradeAsync(request, currentUser.UserId);
        return result.Success ? CreatedAtAction(nameof(GetGrade), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.GradesWrite)]
    [ProducesResponseType(typeof(ApiResponse<GradeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GradeDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateGrade(int id, [FromBody] UpdateGradeRequest request)
    {
        var result = await orgService.UpdateGradeAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.GradesWrite)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteGrade(int id)
    {
        var result = await orgService.DeleteGradeAsync(id, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
