using System.Security.Claims;
using HRMS.Constants;
using HRMS.IServices;

namespace HRMS.Services;

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public int? UserId =>
        int.TryParse(User?.FindFirstValue(AppClaimTypes.UserId), out var id) ? id : null;

    public int? EmployeeId =>
        int.TryParse(User?.FindFirstValue(AppClaimTypes.EmployeeId), out var id) ? id : null;

    public string Username =>
        User?.FindFirstValue(AppClaimTypes.Username) ?? string.Empty;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;

    public bool HasPermission(string permission) =>
        User?.Claims.Any(c => c.Type == AppClaimTypes.Permission && c.Value == permission) ?? false;
}
