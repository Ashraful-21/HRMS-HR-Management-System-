using HRMS.Dtos.Common;
using HRMS.Dtos.Users;

namespace HRMS.IServices;

public interface IAdminService
{
    // Users
    Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(PagedQuery query);
    Task<ApiResponse<UserDto>> GetUserByIdAsync(int id);
    Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserRequest request, int? currentUserId);
    Task<ApiResponse<UserDto>> UpdateUserAsync(int id, UpdateUserRequest request, int? currentUserId);
    Task<ApiResponse> ResetPasswordAsync(int id, ResetUserPasswordRequest request, int? currentUserId);
    Task<ApiResponse> ToggleUserStatusAsync(int id, int? currentUserId);

    // Roles & Permissions
    Task<ApiResponse<List<RoleDto>>> GetRolesAsync();
    Task<ApiResponse<List<string>>> GetAllPermissionsAsync();
    Task<ApiResponse<RoleDto>> UpdateRolePermissionsAsync(int roleId, UpdateRolePermissionsRequest request, int? currentUserId);

    // Settings
    Task<ApiResponse<List<SystemSettingDto>>> GetSettingsAsync(string? category = null);
    Task<ApiResponse<SystemSettingDto>> UpdateSettingAsync(UpdateSettingRequest request, int? currentUserId);
}
