using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace HRMS.Hubs;

/// <summary>
/// Real-time notification hub. Clients connect to /hubs/notifications.
/// The JWT token is passed as ?access_token= query string for browser clients.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>Mark a notification as read (called from client).</summary>
    public async Task MarkRead(int notificationId)
    {
        // Services can call Clients.Group($"user-{userId}").SendAsync("NotificationRead", id)
        await Task.CompletedTask;
    }
}
