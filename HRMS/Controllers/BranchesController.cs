using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Common;
using HRMS.Dtos.Organization;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/branches")]
[Authorize]
[Produces("application/json")]
public class BranchesController(IOrganizationService orgService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.BranchesRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BranchDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBranches([FromQuery] PagedQuery query)
    {
        var result = await orgService.GetBranchesAsync(query);
        return Ok(result);
    }

    [HttpGet("all")]
    [HasPermission(Permissions.BranchesRead)]
    [ProducesResponseType(typeof(ApiResponse<List<BranchDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllBranches()
    {
        var result = await orgService.GetAllBranchesAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.BranchesRead)]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBranch(int id)
    {
        var result = await orgService.GetBranchByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.BranchesWrite)]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request)
    {
        var result = await orgService.CreateBranchAsync(request, currentUser.UserId);
        return result.Success ? CreatedAtAction(nameof(GetBranch), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.BranchesWrite)]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBranch(int id, [FromBody] UpdateBranchRequest request)
    {
        var result = await orgService.UpdateBranchAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.BranchesDelete)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteBranch(int id)
    {
        var result = await orgService.DeleteBranchAsync(id, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
