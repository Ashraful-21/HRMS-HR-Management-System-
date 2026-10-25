using HRMS.Dtos.Common;
using HRMS.Dtos.Notifications;

namespace HRMS.IServices;

public interface ICommunicationService
{
    // Announcements
    Task<ApiResponse<List<AnnouncementDto>>> GetAnnouncementsAsync(bool includeUnpublished = false, int? branchId = null, int? departmentId = null);
    Task<ApiResponse<AnnouncementDto>> GetAnnouncementByIdAsync(int id);
    Task<ApiResponse<AnnouncementDto>> CreateAnnouncementAsync(CreateAnnouncementRequest request, int? userId);
    Task<ApiResponse<AnnouncementDto>> UpdateAnnouncementAsync(int id, UpdateAnnouncementRequest request, int? userId);
    Task<ApiResponse> DeleteAnnouncementAsync(int id, int? userId);

    // Notifications
    Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);
    Task<ApiResponse<int>> GetUnreadCountAsync(int userId);
    Task<ApiResponse> MarkAsReadAsync(int notificationId, int userId);
    Task<ApiResponse> MarkAllAsReadAsync(int userId);
    Task<ApiResponse<NotificationDto>> SendNotificationAsync(CreateNotificationRequest request);
}
