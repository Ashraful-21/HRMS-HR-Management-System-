using HRMS.Constants;
using HRMS.Data;
using HRMS.Dtos.Common;
using HRMS.Dtos.Users;
using HRMS.IServices;
using HRMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Services;

public class AdminService(HrmsDbContext db, IAuditService auditService, ICacheService cache, ILogger<AdminService> logger) : IAdminService
{
    private readonly PasswordHasher<User> _hasher = new();

    public async Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(PagedQuery query)
    {
        var q = db.Users
            .AsNoTracking()
            .Include(u => u.Employee)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            var isActiveSearch = search == "active";
            var isLockedSearch = search == "locked" || search == "inactive" || search == "deactivated";

            q = q.Where(u => u.Username.ToLower().Contains(search)
                          || u.Email.ToLower().Contains(search)
                          || (u.Employee != null && (
                                 u.Employee.FullName.ToLower().Contains(search)
                              || u.Employee.FirstName.ToLower().Contains(search)
                              || u.Employee.LastName.ToLower().Contains(search)
                              || u.Employee.EmployeeCode.ToLower().Contains(search)
                              || (u.Employee.WorkEmail != null && u.Employee.WorkEmail.ToLower().Contains(search))
                             ))
                          || u.UserRoles.Any(ur => !ur.Role.IsDeleted && ur.Role.Name.ToLower().Contains(search))
                          || (isActiveSearch && u.IsActive)
                          || (isLockedSearch && !u.IsActive));
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : (query.PageSize > 100 ? 100 : query.PageSize);

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                EmployeeId = u.EmployeeId,
                EmployeeName = u.Employee != null ? u.Employee.FirstName + " " + u.Employee.LastName : null,
                EmployeeCode = u.Employee != null ? u.Employee.EmployeeCode : null,
                IsActive = u.IsActive,
                MustChangePassword = u.MustChangePassword,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt,
                Roles = u.UserRoles.Where(r => !r.Role.IsDeleted).Select(r => r.Role.Name).ToList()
            })
            .ToListAsync();

        return ApiResponse<PagedResult<UserDto>>.Ok(new PagedResult<UserDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(int id)
    {
        var u = await db.Users
            .AsNoTracking()
            .Include(x => x.Employee)
            .Include(x => x.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (u == null) return ApiResponse<UserDto>.Fail("User not found.");

        return ApiResponse<UserDto>.Ok(new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            EmployeeId = u.EmployeeId,
            EmployeeName = u.Employee != null ? u.Employee.FirstName + " " + u.Employee.LastName : null,
            EmployeeCode = u.Employee?.EmployeeCode,
            IsActive = u.IsActive,
            MustChangePassword = u.MustChangePassword,
            LastLoginAt = u.LastLoginAt,
            CreatedAt = u.CreatedAt,
            Roles = u.UserRoles.Where(r => !r.Role.IsDeleted).Select(r => r.Role.Name).ToList()
        });
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserRequest request, int? currentUserId)
    {
        var exists = await db.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email);
        if (exists) return ApiResponse<UserDto>.Fail("Username or email already exists.");

        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = string.Empty,
            EmployeeId = request.EmployeeId,
            IsActive = true,
            MustChangePassword = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        if (request.Roles.Count > 0)
        {
            var roles = await db.Roles.Where(r => request.Roles.Contains(r.Name) && !r.IsDeleted).ToListAsync();
            foreach (var r in roles)
            {
                db.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = r.Id,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = currentUserId
                });
            }
            await db.SaveChangesAsync();
        }

        await auditService.LogAsync(AuditActions.Create, "Users", user.Id.ToString(), null, new { user.Username, user.Email });
        return await GetUserByIdAsync(user.Id);
    }

    public async Task<ApiResponse<UserDto>> UpdateUserAsync(int id, UpdateUserRequest request, int? currentUserId)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null) return ApiResponse<UserDto>.Fail("User not found.");

        user.Email = request.Email.Trim().ToLower();
        user.IsActive = request.IsActive;
        user.MustChangePassword = request.MustChangePassword;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUserId;

        // Update roles
        db.UserRoles.RemoveRange(user.UserRoles);
        if (request.Roles.Count > 0)
        {
            var roles = await db.Roles.Where(r => request.Roles.Contains(r.Name) && !r.IsDeleted).ToListAsync();
            foreach (var r in roles)
            {
                db.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = r.Id,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = currentUserId
                });
            }
        }

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Users", user.Id.ToString(), null, new { user.Email, user.IsActive });
        return await GetUserByIdAsync(user.Id);
    }

    public async Task<ApiResponse> ResetPasswordAsync(int id, ResetUserPasswordRequest request, int? currentUserId)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user == null) return ApiResponse.Fail("User not found.");

        user.PasswordHash = _hasher.HashPassword(user, request.NewPassword);
        user.MustChangePassword = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUserId;

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Users", user.Id.ToString(), null, new { Action = "PasswordReset" });
        return ApiResponse.Ok("Password reset successfully.");
    }

    public async Task<ApiResponse> ToggleUserStatusAsync(int id, int? currentUserId)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user == null) return ApiResponse.Fail("User not found.");

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUserId;

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Users", user.Id.ToString(), null, new { user.IsActive });
        return ApiResponse.Ok($"User status changed to {(user.IsActive ? "Active" : "Inactive")}.");
    }

    public async Task<ApiResponse<List<RoleDto>>> GetRolesAsync()
    {
        return await cache.GetOrCreateAsync("admin:roles:all", async () =>
        {
            var list = await db.Roles
                .AsNoTracking()
                .Include(r => r.UserRoles)
                .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.Name)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    IsSystem = r.IsSystem,
                    UsersCount = r.UserRoles.Count,
                    Permissions = r.RolePermissions.Select(rp => rp.Permission.Code).ToList()
                })
                .ToListAsync();

            return ApiResponse<List<RoleDto>>.Ok(list);
        }, TimeSpan.FromHours(1));
    }

    public async Task<ApiResponse<List<string>>> GetAllPermissionsAsync()
    {
        return await cache.GetOrCreateAsync("admin:permissions:all", async () =>
        {
            var perms = await db.Permissions
                .AsNoTracking()
                .OrderBy(p => p.Module).ThenBy(p => p.Code)
                .Select(p => p.Code)
                .ToListAsync();

            return ApiResponse<List<string>>.Ok(perms);
        }, TimeSpan.FromHours(2));
    }

    public async Task<ApiResponse<RoleDto>> UpdateRolePermissionsAsync(int roleId, UpdateRolePermissionsRequest request, int? currentUserId)
    {
        var role = await db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted);

        if (role == null) return ApiResponse<RoleDto>.Fail("Role not found.");

        db.RolePermissions.RemoveRange(role.RolePermissions);

        var validPerms = await db.Permissions
            .Where(p => request.PermissionCodes.Contains(p.Code))
            .ToListAsync();

        foreach (var p in validPerms)
        {
            db.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = p.Id,
                GrantedAt = DateTime.UtcNow,
                GrantedBy = currentUserId
            });
        }

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Roles", role.Id.ToString(), null, new { PermissionsCount = validPerms.Count });

        // Invalidate cached roles
        await cache.RemoveAsync("admin:roles:all");

        var updated = await db.Roles
            .AsNoTracking()
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstAsync(r => r.Id == role.Id);

        return ApiResponse<RoleDto>.Ok(new RoleDto
        {
            Id = updated.Id,
            Name = updated.Name,
            Description = updated.Description,
            IsSystem = updated.IsSystem,
            UsersCount = updated.UserRoles.Count,
            Permissions = updated.RolePermissions.Select(rp => rp.Permission.Code).ToList()
        });
    }

    public async Task<ApiResponse<List<SystemSettingDto>>> GetSettingsAsync(string? category = null)
    {
        var cacheKey = $"admin:settings:{category ?? "all"}";
        return await cache.GetOrCreateAsync(cacheKey, async () =>
        {
            var q = db.SystemSettings.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(category))
                q = q.Where(s => s.Category == category);

            var list = await q
                .OrderBy(s => s.Category).ThenBy(s => s.Key)
                .Select(s => new SystemSettingDto
                {
                    Id = s.Id,
                    Key = s.Key,
                    Value = s.Value,
                    Category = s.Category,
                    DataType = s.DataType,
                    Description = s.Description,
                    IsPublic = s.IsPublic
                })
                .ToListAsync();

            return ApiResponse<List<SystemSettingDto>>.Ok(list);
        }, TimeSpan.FromHours(1));
    }

    public async Task<ApiResponse<SystemSettingDto>> UpdateSettingAsync(UpdateSettingRequest request, int? currentUserId)
    {
        var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == request.Key);
        if (setting == null) return ApiResponse<SystemSettingDto>.Fail("Setting key not found.");

        setting.Value = request.Value;
        setting.UpdatedAt = DateTime.UtcNow;
        setting.UpdatedBy = currentUserId;

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "SystemSettings", setting.Id.ToString(), null, new { setting.Key, setting.Value });

        // Invalidate cached settings
        await cache.RemoveAsync("admin:settings:all");
        if (!string.IsNullOrWhiteSpace(setting.Category))
            await cache.RemoveAsync($"admin:settings:{setting.Category}");

        return ApiResponse<SystemSettingDto>.Ok(new SystemSettingDto
        {
            Id = setting.Id,
            Key = setting.Key,
            Value = setting.Value,
            Category = setting.Category,
            DataType = setting.DataType,
            Description = setting.Description,
            IsPublic = setting.IsPublic
        });
    }
}
