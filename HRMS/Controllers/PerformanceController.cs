using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Common;
using HRMS.Dtos.Performance;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/performance")]
[Authorize]
[Produces("application/json")]
public class PerformanceController(IPerformanceService service, ICurrentUserService currentUser) : ControllerBase
{
    // === REVIEW CYCLES ===
    [HttpGet("cycles")]
    [HasPermission(Permissions.PerformanceRead)]
    public async Task<IActionResult> GetReviewCycles([FromQuery] int? year)
    {
        var result = await service.GetReviewCyclesAsync(year);
        return Ok(result);
    }

    [HttpGet("cycles/{id:int}")]
    [HasPermission(Permissions.PerformanceRead)]
    public async Task<IActionResult> GetReviewCycle(int id)
    {
        var result = await service.GetReviewCycleByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("cycles")]
    [HasPermission(Permissions.PerformanceWrite)]
    public async Task<IActionResult> CreateReviewCycle([FromBody] CreateReviewCycleRequest request)
    {
        var result = await service.CreateReviewCycleAsync(request, currentUser.UserId);
        return result.Success ? CreatedAtAction(nameof(GetReviewCycle), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("cycles/{id:int}")]
    [HasPermission(Permissions.PerformanceWrite)]
    public async Task<IActionResult> UpdateReviewCycle(int id, [FromBody] UpdateReviewCycleRequest request)
    {
        var result = await service.UpdateReviewCycleAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // === REVIEWS ===
    [HttpGet("reviews")]
    [HasPermission(Permissions.PerformanceRead)]
    public async Task<IActionResult> GetReviews([FromQuery] PagedQuery query, [FromQuery] int? cycleId, [FromQuery] int? employeeId, [FromQuery] int? reviewerId, [FromQuery] string? status)
    {
        var result = await service.GetPerformanceReviewsAsync(query, cycleId, employeeId, reviewerId, status);
        return Ok(result);
    }

    [HttpGet("reviews/{id:int}")]
    [HasPermission(Permissions.PerformanceRead)]
    public async Task<IActionResult> GetReview(int id)
    {
        var result = await service.GetPerformanceReviewByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("reviews")]
    [HasPermission(Permissions.PerformanceWrite)]
    public async Task<IActionResult> CreateReview([FromBody] CreatePerformanceReviewRequest request)
    {
        var result = await service.CreatePerformanceReviewAsync(request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("reviews/{id:int}/self-submit")]
    [HasPermission(Permissions.PerformanceWrite)]
    public async Task<IActionResult> SubmitSelfReview(int id, [FromBody] SubmitSelfReviewRequest request)
    {
        var result = await service.SubmitSelfReviewAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("reviews/{id:int}/manager-submit")]
    [HasPermission(Permissions.PerformanceWrite)]
    public async Task<IActionResult> SubmitManagerReview(int id, [FromBody] SubmitManagerReviewRequest request)
    {
        var result = await service.SubmitManagerReviewAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // === GOALS ===
    [HttpPost("reviews/{reviewId:int}/goals")]
    [HasPermission(Permissions.PerformanceWrite)]
    public async Task<IActionResult> AddGoal(int reviewId, [FromBody] AddGoalRequest request)
    {
        var result = await service.AddGoalAsync(reviewId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("goals/{goalId:int}")]
    [HasPermission(Permissions.PerformanceWrite)]
    public async Task<IActionResult> DeleteGoal(int goalId)
    {
        var result = await service.DeleteGoalAsync(goalId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
