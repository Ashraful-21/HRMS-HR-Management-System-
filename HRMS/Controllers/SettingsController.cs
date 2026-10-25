using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Users;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/settings")]
[Authorize]
[Produces("application/json")]
public class SettingsController(IAdminService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.SettingsRead)]
    public async Task<IActionResult> GetSettings([FromQuery] string? category)
    {
        var result = await service.GetSettingsAsync(category);
        return Ok(result);
    }

    [HttpPut]
    [HasPermission(Permissions.SettingsWrite)]
    public async Task<IActionResult> UpdateSetting([FromBody] UpdateSettingRequest request)
    {
        var result = await service.UpdateSettingAsync(request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
