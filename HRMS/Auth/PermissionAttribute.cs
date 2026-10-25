using Microsoft.AspNetCore.Authorization;

namespace HRMS.Auth;

/// <summary>
/// Apply to controller actions: [HasPermission(Permissions.EmployeesRead)]
/// Uses the PermissionAuthorizationHandler to verify claims.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class HasPermissionAttribute(string permission)
    : AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
{
    public string Permission { get; } = permission;

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return this;
    }
}

public class PermissionAuthorizationHandler
    : AuthorizationHandler<HasPermissionAttribute>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HasPermissionAttribute requirement)
    {
        var hasClaim = context.User.Claims
            .Any(c => c.Type  == Constants.AppClaimTypes.Permission
                   && c.Value == requirement.Permission);

        if (hasClaim)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
