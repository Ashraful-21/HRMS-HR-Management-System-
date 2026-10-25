using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Notifications;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/announcements")]
[Authorize]
[Produces("application/json")]
public class AnnouncementsController(ICommunicationService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.AnnouncementsRead)]
    public async Task<IActionResult> GetAnnouncements([FromQuery] bool includeUnpublished = false, [FromQuery] int? branchId = null, [FromQuery] int? departmentId = null)
    {
        var result = await service.GetAnnouncementsAsync(includeUnpublished, branchId, departmentId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.AnnouncementsRead)]
    public async Task<IActionResult> GetAnnouncement(int id)
    {
        var result = await service.GetAnnouncementByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.AnnouncementsWrite)]
    public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementRequest request)
    {
        var result = await service.CreateAnnouncementAsync(request, currentUser.UserId);
        return result.Success ? CreatedAtAction(nameof(GetAnnouncement), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.AnnouncementsWrite)]
    public async Task<IActionResult> UpdateAnnouncement(int id, [FromBody] UpdateAnnouncementRequest request)
    {
        var result = await service.UpdateAnnouncementAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.AnnouncementsWrite)]
    public async Task<IActionResult> DeleteAnnouncement(int id)
    {
        var result = await service.DeleteAnnouncementAsync(id, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
