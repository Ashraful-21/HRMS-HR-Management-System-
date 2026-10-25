using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Dashboard;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
[Produces("application/json")]
public class DashboardController(IDashboardService service) : ControllerBase
{
    [HttpGet("summary")]
    [HasPermission(Permissions.EmployeesRead)] // All authenticated users with view access
    public async Task<IActionResult> GetSummary()
    {
        var result = await service.GetDashboardSummaryAsync();
        return Ok(result);
    }
}
