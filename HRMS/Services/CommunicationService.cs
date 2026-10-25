using HRMS.Constants;
using HRMS.Data;
using HRMS.Dtos.Common;
using HRMS.Dtos.Notifications;
using HRMS.Hubs;
using HRMS.IServices;
using HRMS.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Services;

public class CommunicationService(
    HrmsDbContext db,
    IAuditService auditService,
    IHubContext<NotificationHub> hubContext,
    ILogger<CommunicationService> logger) : ICommunicationService
{
    public async Task<ApiResponse<List<AnnouncementDto>>> GetAnnouncementsAsync(bool includeUnpublished = false, int? branchId = null, int? departmentId = null)
    {
        var now = DateTime.UtcNow;
        var q = db.Announcements
            .AsNoTracking()
            .Include(a => a.Department)
            .Include(a => a.Branch)
            .Where(a => !a.IsDeleted);

        if (!includeUnpublished)
        {
            q = q.Where(a => a.IsPublished && a.PublishFrom <= now && (a.PublishTo == null || a.PublishTo >= now));
        }

        if (branchId.HasValue)
            q = q.Where(a => a.BranchId == null || a.BranchId == branchId.Value);

        if (departmentId.HasValue)
            q = q.Where(a => a.DepartmentId == null || a.DepartmentId == departmentId.Value);

        var list = await q
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.PublishFrom)
            .Select(a => new AnnouncementDto
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                DepartmentId = a.DepartmentId,
                DepartmentName = a.Department != null ? a.Department.Name : null,
                BranchId = a.BranchId,
                BranchName = a.Branch != null ? a.Branch.Name : null,
                IsPinned = a.IsPinned,
                PublishFrom = a.PublishFrom,
                PublishTo = a.PublishTo,
                IsPublished = a.IsPublished,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<AnnouncementDto>>.Ok(list);
    }

    public async Task<ApiResponse<AnnouncementDto>> GetAnnouncementByIdAsync(int id)
    {
        var a = await db.Announcements
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Branch)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (a == null) return ApiResponse<AnnouncementDto>.Fail("Announcement not found.");

        return ApiResponse<AnnouncementDto>.Ok(new AnnouncementDto
        {
            Id = a.Id,
            Title = a.Title,
            Content = a.Content,
            DepartmentId = a.DepartmentId,
            DepartmentName = a.Department?.Name,
            BranchId = a.BranchId,
            BranchName = a.Branch?.Name,
            IsPinned = a.IsPinned,
            PublishFrom = a.PublishFrom,
            PublishTo = a.PublishTo,
            IsPublished = a.IsPublished,
            CreatedAt = a.CreatedAt
        });
    }

    public async Task<ApiResponse<AnnouncementDto>> CreateAnnouncementAsync(CreateAnnouncementRequest request, int? userId)
    {
        var entity = new Announcement
        {
            Title = request.Title,
            Content = request.Content,
            DepartmentId = request.DepartmentId,
            BranchId = request.BranchId,
            IsPinned = request.IsPinned,
            PublishFrom = request.PublishFrom ?? DateTime.UtcNow,
            PublishTo = request.PublishTo,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        db.Announcements.Add(entity);
        await db.SaveChangesAsync();

        await auditService.LogAsync(AuditActions.Create, "Announcements", entity.Id.ToString(), null, entity);

        // Broadcast announcement alert to all users via SignalR
        try
        {
            await hubContext.Clients.All.SendAsync("AnnouncementCreated", new
            {
                entity.Id,
                entity.Title,
                entity.Content,
                entity.IsPinned,
                entity.CreatedAt
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast announcement to SignalR hub.");
        }

        return await GetAnnouncementByIdAsync(entity.Id);
    }

    public async Task<ApiResponse<AnnouncementDto>> UpdateAnnouncementAsync(int id, UpdateAnnouncementRequest request, int? userId)
    {
        var entity = await db.Announcements.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) return ApiResponse<AnnouncementDto>.Fail("Announcement not found.");

        entity.Title = request.Title;
        entity.Content = request.Content;
        entity.DepartmentId = request.DepartmentId;
        entity.BranchId = request.BranchId;
        entity.IsPinned = request.IsPinned;
        entity.IsPublished = request.IsPublished;
        entity.PublishTo = request.PublishTo;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Announcements", entity.Id.ToString(), null, entity);
        return await GetAnnouncementByIdAsync(entity.Id);
    }

    public async Task<ApiResponse> DeleteAnnouncementAsync(int id, int? userId)
    {
        var entity = await db.Announcements.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) return ApiResponse.Fail("Announcement not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Delete, "Announcements", id.ToString(), null, new { IsDeleted = true });
        return ApiResponse.Ok("Announcement deleted successfully.");
    }

    public async Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(int userId, bool unreadOnly = false)
    {
        var q = db.Notifications.AsNoTracking().Where(n => n.UserId == userId);

        if (unreadOnly)
            q = q.Where(n => !n.IsRead);

        var list = await q
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                EntityType = n.EntityType,
                EntityId = n.EntityId,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<NotificationDto>>.Ok(list);
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(int userId)
    {
        var count = await db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        return ApiResponse<int>.Ok(count);
    }

    public async Task<ApiResponse> MarkAsReadAsync(int notificationId, int userId)
    {
        var notif = await db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
        if (notif == null) return ApiResponse.Fail("Notification not found.");

        notif.IsRead = true;
        notif.ReadAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return ApiResponse.Ok("Marked as read.");
    }

    public async Task<ApiResponse> MarkAllAsReadAsync(int userId)
    {
        var list = await db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in list)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return ApiResponse.Ok("All notifications marked as read.");
    }

    public async Task<ApiResponse<NotificationDto>> SendNotificationAsync(CreateNotificationRequest request)
    {
        var notif = new Notification
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        db.Notifications.Add(notif);
        await db.SaveChangesAsync();

        var dto = new NotificationDto
        {
            Id = notif.Id,
            UserId = notif.UserId,
            Title = notif.Title,
            Message = notif.Message,
            Type = notif.Type,
            EntityType = notif.EntityType,
            EntityId = notif.EntityId,
            IsRead = false,
            CreatedAt = notif.CreatedAt
        };

        try
        {
            await hubContext.Clients.Group($"user-{request.UserId}").SendAsync("ReceiveNotification", dto);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send SignalR notification to user-{UserId}", request.UserId);
        }

        return ApiResponse<NotificationDto>.Ok(dto);
    }
}
