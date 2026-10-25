using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Notifications;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationsController(ICommunicationService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.NotificationsRead)]
    public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly = false)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await service.GetUserNotificationsAsync(userId, unreadOnly);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    [HasPermission(Permissions.NotificationsRead)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = currentUser.UserId ?? 0;
        var result = await service.GetUnreadCountAsync(userId);
        return Ok(result);
    }

    [HttpPost("{id:int}/read")]
    [HasPermission(Permissions.NotificationsRead)]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = currentUser.UserId ?? 0;
        var result = await service.MarkAsReadAsync(id, userId);
        return Ok(result);
    }

    [HttpPost("read-all")]
    [HasPermission(Permissions.NotificationsRead)]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = currentUser.UserId ?? 0;
        var result = await service.MarkAllAsReadAsync(userId);
        return Ok(result);
    }

    [HttpPost("send")]
    [HasPermission(Permissions.AnnouncementsWrite)] // Managers/HR can send notifications
    public async Task<IActionResult> SendNotification([FromBody] CreateNotificationRequest request)
    {
        var result = await service.SendNotificationAsync(request);
        return Ok(result);
    }
}
