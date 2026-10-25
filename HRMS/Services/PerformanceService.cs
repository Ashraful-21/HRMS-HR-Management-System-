using HRMS.Constants;
using HRMS.Data;
using HRMS.Dtos.Common;
using HRMS.Dtos.Performance;
using HRMS.IServices;
using HRMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Services;

public class PerformanceService(HrmsDbContext db, IAuditService auditService, ILogger<PerformanceService> logger) : IPerformanceService
{
    public async Task<ApiResponse<List<ReviewCycleDto>>> GetReviewCyclesAsync(int? year = null)
    {
        var q = db.ReviewCycles.AsNoTracking().Where(r => !r.IsDeleted);
        if (year.HasValue) q = q.Where(r => r.Year == year.Value);

        var list = await q
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.StartDate)
            .Select(r => new ReviewCycleDto
            {
                Id = r.Id,
                Name = r.Name,
                Year = r.Year,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Status = r.Status,
                TotalReviews = r.PerformanceReviews.Count(p => !p.IsDeleted),
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<ReviewCycleDto>>.Ok(list);
    }

    public async Task<ApiResponse<ReviewCycleDto>> GetReviewCycleByIdAsync(int id)
    {
        var r = await db.ReviewCycles
            .AsNoTracking()
            .Include(x => x.PerformanceReviews)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (r == null) return ApiResponse<ReviewCycleDto>.Fail("Review cycle not found.");

        return ApiResponse<ReviewCycleDto>.Ok(new ReviewCycleDto
        {
            Id = r.Id,
            Name = r.Name,
            Year = r.Year,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            Status = r.Status,
            TotalReviews = r.PerformanceReviews.Count(p => !p.IsDeleted),
            CreatedAt = r.CreatedAt
        });
    }

    public async Task<ApiResponse<ReviewCycleDto>> CreateReviewCycleAsync(CreateReviewCycleRequest request, int? userId)
    {
        var entity = new ReviewCycle
        {
            Name = request.Name,
            Year = request.Year,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        db.ReviewCycles.Add(entity);
        await db.SaveChangesAsync();

        await auditService.LogAsync(AuditActions.Create, "ReviewCycles", entity.Id.ToString(), null, entity);
        return await GetReviewCycleByIdAsync(entity.Id);
    }

    public async Task<ApiResponse<ReviewCycleDto>> UpdateReviewCycleAsync(int id, UpdateReviewCycleRequest request, int? userId)
    {
        var entity = await db.ReviewCycles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) return ApiResponse<ReviewCycleDto>.Fail("Review cycle not found.");

        entity.Name = request.Name;
        entity.Year = request.Year;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "ReviewCycles", entity.Id.ToString(), null, entity);
        return await GetReviewCycleByIdAsync(entity.Id);
    }

    public async Task<ApiResponse<PagedResult<PerformanceReviewDto>>> GetPerformanceReviewsAsync(
        PagedQuery query, int? cycleId = null, int? employeeId = null, int? reviewerId = null, string? status = null)
    {
        var q = db.PerformanceReviews
            .AsNoTracking()
            .Include(p => p.ReviewCycle)
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Include(p => p.Employee).ThenInclude(e => e.Designation)
            .Include(p => p.Reviewer)
            .Include(p => p.PerformanceGoals)
            .Where(p => !p.IsDeleted);

        if (cycleId.HasValue) q = q.Where(p => p.ReviewCycleId == cycleId.Value);
        if (employeeId.HasValue) q = q.Where(p => p.EmployeeId == employeeId.Value);
        if (reviewerId.HasValue) q = q.Where(p => p.ReviewerId == reviewerId.Value);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(p => p.Status == status);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            q = q.Where(p => p.Employee.FirstName.ToLower().Contains(search) || p.Employee.LastName.ToLower().Contains(search) || p.Employee.EmployeeCode.ToLower().Contains(search));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new PerformanceReviewDto
            {
                Id = p.Id,
                ReviewCycleId = p.ReviewCycleId,
                ReviewCycleName = p.ReviewCycle.Name,
                EmployeeId = p.EmployeeId,
                EmployeeName = p.Employee.FirstName + " " + p.Employee.LastName,
                EmployeeCode = p.Employee.EmployeeCode,
                DepartmentName = p.Employee.Department.Name,
                DesignationTitle = p.Employee.Designation.Title,
                ReviewerId = p.ReviewerId,
                ReviewerName = p.Reviewer.FirstName + " " + p.Reviewer.LastName,
                Status = p.Status,
                SelfRating = p.SelfRating,
                ManagerRating = p.ManagerRating,
                FinalRating = p.FinalRating,
                SelfComments = p.SelfComments,
                ManagerComments = p.ManagerComments,
                SelfSubmittedAt = p.SelfSubmittedAt,
                ManagerSubmittedAt = p.ManagerSubmittedAt,
                CreatedAt = p.CreatedAt,
                Goals = p.PerformanceGoals.Select(g => new PerformanceGoalDto
                {
                    Id = g.Id,
                    ReviewId = g.ReviewId,
                    GoalDescription = g.GoalDescription,
                    Target = g.Target,
                    Achievement = g.Achievement,
                    Weight = g.Weight,
                    SelfScore = g.SelfScore,
                    ManagerScore = g.ManagerScore,
                    CreatedAt = g.CreatedAt
                }).ToList()
            })
            .ToListAsync();

        return ApiResponse<PagedResult<PerformanceReviewDto>>.Ok(new PagedResult<PerformanceReviewDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<PerformanceReviewDto>> GetPerformanceReviewByIdAsync(int id)
    {
        var p = await db.PerformanceReviews
            .AsNoTracking()
            .Include(x => x.ReviewCycle)
            .Include(x => x.Employee).ThenInclude(e => e.Department)
            .Include(x => x.Employee).ThenInclude(e => e.Designation)
            .Include(x => x.Reviewer)
            .Include(x => x.PerformanceGoals)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (p == null) return ApiResponse<PerformanceReviewDto>.Fail("Performance review not found.");

        return ApiResponse<PerformanceReviewDto>.Ok(new PerformanceReviewDto
        {
            Id = p.Id,
            ReviewCycleId = p.ReviewCycleId,
            ReviewCycleName = p.ReviewCycle.Name,
            EmployeeId = p.EmployeeId,
            EmployeeName = p.Employee.FirstName + " " + p.Employee.LastName,
            EmployeeCode = p.Employee.EmployeeCode,
            DepartmentName = p.Employee.Department.Name,
            DesignationTitle = p.Employee.Designation.Title,
            ReviewerId = p.ReviewerId,
            ReviewerName = p.Reviewer.FirstName + " " + p.Reviewer.LastName,
            Status = p.Status,
            SelfRating = p.SelfRating,
            ManagerRating = p.ManagerRating,
            FinalRating = p.FinalRating,
            SelfComments = p.SelfComments,
            ManagerComments = p.ManagerComments,
            SelfSubmittedAt = p.SelfSubmittedAt,
            ManagerSubmittedAt = p.ManagerSubmittedAt,
            CreatedAt = p.CreatedAt,
            Goals = p.PerformanceGoals.Select(g => new PerformanceGoalDto
            {
                Id = g.Id,
                ReviewId = g.ReviewId,
                GoalDescription = g.GoalDescription,
                Target = g.Target,
                Achievement = g.Achievement,
                Weight = g.Weight,
                SelfScore = g.SelfScore,
                ManagerScore = g.ManagerScore,
                CreatedAt = g.CreatedAt
            }).ToList()
        });
    }

    public async Task<ApiResponse<PerformanceReviewDto>> CreatePerformanceReviewAsync(CreatePerformanceReviewRequest request, int? userId)
    {
        var exists = await db.PerformanceReviews.AnyAsync(p => p.ReviewCycleId == request.ReviewCycleId && p.EmployeeId == request.EmployeeId && !p.IsDeleted);
        if (exists) return ApiResponse<PerformanceReviewDto>.Fail("A review already exists for this employee in this review cycle.");

        var review = new PerformanceReview
        {
            ReviewCycleId = request.ReviewCycleId,
            EmployeeId = request.EmployeeId,
            ReviewerId = request.ReviewerId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        db.PerformanceReviews.Add(review);
        await db.SaveChangesAsync();

        return await GetPerformanceReviewByIdAsync(review.Id);
    }

    public async Task<ApiResponse<PerformanceReviewDto>> SubmitSelfReviewAsync(int id, SubmitSelfReviewRequest request, int? userId)
    {
        var review = await db.PerformanceReviews
            .Include(p => p.PerformanceGoals)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (review == null) return ApiResponse<PerformanceReviewDto>.Fail("Performance review not found.");

        review.SelfRating = request.SelfRating;
        review.SelfComments = request.SelfComments;
        review.SelfSubmittedAt = DateTime.UtcNow;
        review.Status = "SelfSubmitted";
        review.UpdatedAt = DateTime.UtcNow;
        review.UpdatedBy = userId;

        if (request.Goals != null && request.Goals.Count > 0)
        {
            foreach (var g in request.Goals)
            {
                db.PerformanceGoals.Add(new PerformanceGoal
                {
                    ReviewId = review.Id,
                    GoalDescription = g.GoalDescription,
                    Target = g.Target,
                    Achievement = g.Achievement,
                    Weight = g.Weight,
                    SelfScore = g.SelfScore,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "PerformanceReviews", review.Id.ToString(), null, new { review.Status, review.SelfRating });
        return await GetPerformanceReviewByIdAsync(review.Id);
    }

    public async Task<ApiResponse<PerformanceReviewDto>> SubmitManagerReviewAsync(int id, SubmitManagerReviewRequest request, int? userId)
    {
        var review = await db.PerformanceReviews
            .Include(p => p.PerformanceGoals)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (review == null) return ApiResponse<PerformanceReviewDto>.Fail("Performance review not found.");

        review.ManagerRating = request.ManagerRating;
        review.ManagerComments = request.ManagerComments;
        review.FinalRating = request.FinalRating;
        review.ManagerSubmittedAt = DateTime.UtcNow;
        review.Status = "Finalized";
        review.UpdatedAt = DateTime.UtcNow;
        review.UpdatedBy = userId;

        if (request.GoalScores != null)
        {
            foreach (var gs in request.GoalScores)
            {
                var goal = review.PerformanceGoals.FirstOrDefault(x => x.Id == gs.GoalId);
                if (goal != null)
                {
                    goal.ManagerScore = gs.ManagerScore;
                }
            }
        }

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "PerformanceReviews", review.Id.ToString(), null, new { review.Status, review.FinalRating });
        return await GetPerformanceReviewByIdAsync(review.Id);
    }

    public async Task<ApiResponse<PerformanceGoalDto>> AddGoalAsync(int reviewId, AddGoalRequest request)
    {
        var goal = new PerformanceGoal
        {
            ReviewId = reviewId,
            GoalDescription = request.GoalDescription,
            Target = request.Target,
            Achievement = request.Achievement,
            Weight = request.Weight,
            SelfScore = request.SelfScore,
            CreatedAt = DateTime.UtcNow
        };

        db.PerformanceGoals.Add(goal);
        await db.SaveChangesAsync();

        return ApiResponse<PerformanceGoalDto>.Ok(new PerformanceGoalDto
        {
            Id = goal.Id,
            ReviewId = goal.ReviewId,
            GoalDescription = goal.GoalDescription,
            Target = goal.Target,
            Achievement = goal.Achievement,
            Weight = goal.Weight,
            SelfScore = goal.SelfScore,
            CreatedAt = goal.CreatedAt
        });
    }

    public async Task<ApiResponse> DeleteGoalAsync(int goalId)
    {
        var goal = await db.PerformanceGoals.FindAsync(goalId);
        if (goal == null) return ApiResponse.Fail("Goal not found.");

        db.PerformanceGoals.Remove(goal);
        await db.SaveChangesAsync();
        return ApiResponse.Ok("Goal removed successfully.");
    }
}
