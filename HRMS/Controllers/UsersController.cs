using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Common;
using HRMS.Dtos.Users;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
[Produces("application/json")]
public class UsersController(IAdminService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.UsersRead)]
    public async Task<IActionResult> GetUsers([FromQuery] PagedQuery query)
    {
        var result = await service.GetUsersAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.UsersRead)]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await service.GetUserByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.UsersWrite)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await service.CreateUserAsync(request, currentUser.UserId);
        return result.Success ? CreatedAtAction(nameof(GetUser), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.UsersWrite)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var result = await service.UpdateUserAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/reset-password")]
    [HasPermission(Permissions.UsersWrite)]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetUserPasswordRequest request)
    {
        var result = await service.ResetPasswordAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/toggle-status")]
    [HasPermission(Permissions.UsersWrite)]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await service.ToggleUserStatusAsync(id, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
