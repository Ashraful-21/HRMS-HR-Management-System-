using HRMS.Dtos.Common;
using HRMS.Dtos.Performance;

namespace HRMS.IServices;

public interface IPerformanceService
{
    // Review Cycles
    Task<ApiResponse<List<ReviewCycleDto>>> GetReviewCyclesAsync(int? year = null);
    Task<ApiResponse<ReviewCycleDto>> GetReviewCycleByIdAsync(int id);
    Task<ApiResponse<ReviewCycleDto>> CreateReviewCycleAsync(CreateReviewCycleRequest request, int? userId);
    Task<ApiResponse<ReviewCycleDto>> UpdateReviewCycleAsync(int id, UpdateReviewCycleRequest request, int? userId);

    // Performance Reviews
    Task<ApiResponse<PagedResult<PerformanceReviewDto>>> GetPerformanceReviewsAsync(PagedQuery query, int? cycleId = null, int? employeeId = null, int? reviewerId = null, string? status = null);
    Task<ApiResponse<PerformanceReviewDto>> GetPerformanceReviewByIdAsync(int id);
    Task<ApiResponse<PerformanceReviewDto>> CreatePerformanceReviewAsync(CreatePerformanceReviewRequest request, int? userId);
    Task<ApiResponse<PerformanceReviewDto>> SubmitSelfReviewAsync(int id, SubmitSelfReviewRequest request, int? userId);
    Task<ApiResponse<PerformanceReviewDto>> SubmitManagerReviewAsync(int id, SubmitManagerReviewRequest request, int? userId);

    // Goals
    Task<ApiResponse<PerformanceGoalDto>> AddGoalAsync(int reviewId, AddGoalRequest request);
    Task<ApiResponse> DeleteGoalAsync(int goalId);
}
