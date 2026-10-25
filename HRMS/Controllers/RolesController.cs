using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Users;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Authorize]
[Produces("application/json")]
public class RolesController(IAdminService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.RolesRead)]
    public async Task<IActionResult> GetRoles()
    {
        var result = await service.GetRolesAsync();
        return Ok(result);
    }

    [HttpGet("permissions")]
    [HasPermission(Permissions.RolesRead)]
    public async Task<IActionResult> GetAllPermissions()
    {
        var result = await service.GetAllPermissionsAsync();
        return Ok(result);
    }

    [HttpPut("{roleId:int}/permissions")]
    [HasPermission(Permissions.RolesWrite)]
    public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] UpdateRolePermissionsRequest request)
    {
        var result = await service.UpdateRolePermissionsAsync(roleId, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
