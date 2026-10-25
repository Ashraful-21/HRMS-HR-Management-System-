using Hangfire.Dashboard;
using HRMS.Constants;

namespace HRMS.Extensions;

/// <summary>
/// Hangfire dashboard is only accessible by SuperAdmin users.
/// </summary>
public class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole(Roles.SuperAdmin);
    }
}
