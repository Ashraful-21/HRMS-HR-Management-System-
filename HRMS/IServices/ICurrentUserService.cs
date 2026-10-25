using System.Security.Claims;

namespace HRMS.IServices;

/// <summary>
/// Provides the identity of the currently authenticated user from the HTTP context.
/// Services depend on this; never on IHttpContextAccessor directly.
/// </summary>
public interface ICurrentUserService
{
    int?   UserId     { get; }
    int?   EmployeeId { get; }
    string Username   { get; }
    bool   IsAuthenticated { get; }
    bool   HasPermission(string permission);
}
