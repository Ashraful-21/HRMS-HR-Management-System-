using HRMS.Dtos.Common;
using HRMS.Dtos.Recruitment;

namespace HRMS.IServices;

public interface IRecruitmentService
{
    // Job Openings
    Task<ApiResponse<PagedResult<JobOpeningDto>>> GetJobOpeningsAsync(PagedQuery query, string? status = null, int? departmentId = null);
    Task<ApiResponse<JobOpeningDto>> GetJobOpeningByIdAsync(int id);
    Task<ApiResponse<JobOpeningDto>> CreateJobOpeningAsync(CreateJobOpeningRequest request, int? userId);
    Task<ApiResponse<JobOpeningDto>> UpdateJobOpeningAsync(int id, UpdateJobOpeningRequest request, int? userId);
    Task<ApiResponse> DeleteJobOpeningAsync(int id, int? userId);

    // Candidates
    Task<ApiResponse<PagedResult<CandidateDto>>> GetCandidatesAsync(PagedQuery query);
    Task<ApiResponse<CandidateDto>> GetCandidateByIdAsync(int id);
    Task<ApiResponse<CandidateDto>> CreateCandidateAsync(CreateCandidateRequest request, int? userId);

    // Applications & Pipeline
    Task<ApiResponse<List<ApplicationDto>>> GetApplicationsAsync(int? jobOpeningId = null, string? stage = null);
    Task<ApiResponse<ApplicationDto>> CreateApplicationAsync(CreateApplicationRequest request, int? userId);
    Task<ApiResponse<ApplicationDto>> UpdateApplicationStageAsync(int id, UpdateApplicationStageRequest request, int? userId);

    // Interviews
    Task<ApiResponse<List<InterviewDto>>> GetInterviewsAsync(int? applicationId = null, int? interviewerId = null);
    Task<ApiResponse<InterviewDto>> ScheduleInterviewAsync(ScheduleInterviewRequest request, int? userId);
    Task<ApiResponse<InterviewDto>> SubmitInterviewFeedbackAsync(int id, SubmitInterviewFeedbackRequest request, int? userId);

    // Offer Letters
    Task<ApiResponse<List<OfferLetterDto>>> GetOfferLettersAsync(int? applicationId = null);
    Task<ApiResponse<OfferLetterDto>> CreateOfferLetterAsync(CreateOfferLetterRequest request, int? userId);
    Task<ApiResponse<OfferLetterDto>> UpdateOfferStatusAsync(int id, UpdateOfferStatusRequest request, int? userId);
}
