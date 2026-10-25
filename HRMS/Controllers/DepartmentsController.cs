using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Common;
using HRMS.Dtos.Organization;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/departments")]
[Authorize]
[Produces("application/json")]
public class DepartmentsController(IOrganizationService orgService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.DepartmentsRead)]
    [ProducesResponseType(typeof(ApiResponse<List<DepartmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments([FromQuery] int? branchId)
    {
        var result = await orgService.GetDepartmentsAsync(branchId);
        return Ok(result);
    }

    [HttpGet("tree")]
    [HasPermission(Permissions.DepartmentsRead)]
    [ProducesResponseType(typeof(ApiResponse<List<DepartmentTreeNodeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartmentTree([FromQuery] int? branchId)
    {
        var result = await orgService.GetDepartmentTreeAsync(branchId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.DepartmentsRead)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartment(int id)
    {
        var result = await orgService.GetDepartmentByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.DepartmentsWrite)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
        var result = await orgService.CreateDepartmentAsync(request, currentUser.UserId);
        return result.Success ? CreatedAtAction(nameof(GetDepartment), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.DepartmentsWrite)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentRequest request)
    {
        var result = await orgService.UpdateDepartmentAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.DepartmentsDelete)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var result = await orgService.DeleteDepartmentAsync(id, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
