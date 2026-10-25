using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Common;
using HRMS.Dtos.Recruitment;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/recruitment")]
[Authorize]
[Produces("application/json")]
public class RecruitmentController(IRecruitmentService service, ICurrentUserService currentUser) : ControllerBase
{
    // === JOB OPENINGS ===
    [HttpGet("jobs")]
    [HasPermission(Permissions.RecruitmentRead)]
    public async Task<IActionResult> GetJobOpenings([FromQuery] PagedQuery query, [FromQuery] string? status, [FromQuery] int? departmentId)
    {
        var result = await service.GetJobOpeningsAsync(query, status, departmentId);
        return Ok(result);
    }

    [HttpGet("jobs/{id:int}")]
    [HasPermission(Permissions.RecruitmentRead)]
    public async Task<IActionResult> GetJobOpening(int id)
    {
        var result = await service.GetJobOpeningByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("jobs")]
    [HasPermission(Permissions.RecruitmentWrite)]
    public async Task<IActionResult> CreateJobOpening([FromBody] CreateJobOpeningRequest request)
    {
        var result = await service.CreateJobOpeningAsync(request, currentUser.UserId);
        return result.Success ? CreatedAtAction(nameof(GetJobOpening), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("jobs/{id:int}")]
    [HasPermission(Permissions.RecruitmentWrite)]
    public async Task<IActionResult> UpdateJobOpening(int id, [FromBody] UpdateJobOpeningRequest request)
    {
        var result = await service.UpdateJobOpeningAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("jobs/{id:int}")]
    [HasPermission(Permissions.RecruitmentWrite)]
    public async Task<IActionResult> DeleteJobOpening(int id)
    {
        var result = await service.DeleteJobOpeningAsync(id, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // === CANDIDATES ===
    [HttpGet("candidates")]
    [HasPermission(Permissions.RecruitmentRead)]
    public async Task<IActionResult> GetCandidates([FromQuery] PagedQuery query)
    {
        var result = await service.GetCandidatesAsync(query);
        return Ok(result);
    }

    [HttpGet("candidates/{id:int}")]
    [HasPermission(Permissions.RecruitmentRead)]
    public async Task<IActionResult> GetCandidate(int id)
    {
        var result = await service.GetCandidateByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("candidates")]
    [HasPermission(Permissions.RecruitmentWrite)]
    public async Task<IActionResult> CreateCandidate([FromBody] CreateCandidateRequest request)
    {
        var result = await service.CreateCandidateAsync(request, currentUser.UserId);
        return result.Success ? CreatedAtAction(nameof(GetCandidate), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    // === APPLICATIONS ===
    [HttpGet("applications")]
    [HasPermission(Permissions.RecruitmentRead)]
    public async Task<IActionResult> GetApplications([FromQuery] int? jobOpeningId, [FromQuery] string? stage)
    {
        var result = await service.GetApplicationsAsync(jobOpeningId, stage);
        return Ok(result);
    }

    [HttpPost("applications")]
    [HasPermission(Permissions.RecruitmentWrite)]
    public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationRequest request)
    {
        var result = await service.CreateApplicationAsync(request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("applications/{id:int}/stage")]
    [HasPermission(Permissions.RecruitmentWrite)]
    public async Task<IActionResult> UpdateApplicationStage(int id, [FromBody] UpdateApplicationStageRequest request)
    {
        var result = await service.UpdateApplicationStageAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // === INTERVIEWS ===
    [HttpGet("interviews")]
    [HasPermission(Permissions.RecruitmentRead)]
    public async Task<IActionResult> GetInterviews([FromQuery] int? applicationId, [FromQuery] int? interviewerId)
    {
        var result = await service.GetInterviewsAsync(applicationId, interviewerId);
        return Ok(result);
    }

    [HttpPost("interviews")]
    [HasPermission(Permissions.RecruitmentWrite)]
    public async Task<IActionResult> ScheduleInterview([FromBody] ScheduleInterviewRequest request)
    {
        var result = await service.ScheduleInterviewAsync(request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("interviews/{id:int}/feedback")]
    [HasPermission(Permissions.RecruitmentWrite)]
    public async Task<IActionResult> SubmitInterviewFeedback(int id, [FromBody] SubmitInterviewFeedbackRequest request)
    {
        var result = await service.SubmitInterviewFeedbackAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // === OFFER LETTERS ===
    [HttpGet("offers")]
    [HasPermission(Permissions.RecruitmentRead)]
    public async Task<IActionResult> GetOfferLetters([FromQuery] int? applicationId)
    {
        var result = await service.GetOfferLettersAsync(applicationId);
        return Ok(result);
    }

    [HttpPost("offers")]
    [HasPermission(Permissions.RecruitmentWrite)]
    public async Task<IActionResult> CreateOfferLetter([FromBody] CreateOfferLetterRequest request)
    {
        var result = await service.CreateOfferLetterAsync(request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("offers/{id:int}/status")]
    [HasPermission(Permissions.RecruitmentWrite)]
    public async Task<IActionResult> UpdateOfferStatus(int id, [FromBody] UpdateOfferStatusRequest request)
    {
        var result = await service.UpdateOfferStatusAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
