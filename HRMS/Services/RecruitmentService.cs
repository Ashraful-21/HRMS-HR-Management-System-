using HRMS.Constants;
using HRMS.Data;
using HRMS.Dtos.Common;
using HRMS.Dtos.Recruitment;
using HRMS.IServices;
using HRMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Services;

public class RecruitmentService(HrmsDbContext db, IAuditService auditService, ILogger<RecruitmentService> logger) : IRecruitmentService
{
    public async Task<ApiResponse<PagedResult<JobOpeningDto>>> GetJobOpeningsAsync(PagedQuery query, string? status = null, int? departmentId = null)
    {
        var q = db.JobOpenings
            .AsNoTracking()
            .Include(j => j.Department)
            .Include(j => j.Designation)
            .Include(j => j.Branch)
            .Include(j => j.Applications)
            .Where(j => !j.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(j => j.Status == status);

        if (departmentId.HasValue)
            q = q.Where(j => j.DepartmentId == departmentId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            q = q.Where(j => j.Title.ToLower().Contains(search) || j.Department.Name.ToLower().Contains(search));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(j => j.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(j => new JobOpeningDto
            {
                Id = j.Id,
                Title = j.Title,
                DepartmentId = j.DepartmentId,
                DepartmentName = j.Department.Name,
                DesignationId = j.DesignationId,
                DesignationTitle = j.Designation != null ? j.Designation.Title : null,
                BranchId = j.BranchId,
                BranchName = j.Branch.Name,
                Vacancies = j.Vacancies,
                EmploymentType = j.EmploymentType,
                ExperienceMinYrs = j.ExperienceMinYrs,
                ExperienceMaxYrs = j.ExperienceMaxYrs,
                SalaryMin = j.SalaryMin,
                SalaryMax = j.SalaryMax,
                Description = j.Description,
                Requirements = j.Requirements,
                PostedDate = j.PostedDate,
                ClosingDate = j.ClosingDate,
                Status = j.Status,
                ApplicationsCount = j.Applications.Count(a => !a.IsDeleted),
                CreatedAt = j.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<PagedResult<JobOpeningDto>>.Ok(new PagedResult<JobOpeningDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<JobOpeningDto>> GetJobOpeningByIdAsync(int id)
    {
        var j = await db.JobOpenings
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Designation)
            .Include(x => x.Branch)
            .Include(x => x.Applications)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (j == null) return ApiResponse<JobOpeningDto>.Fail("Job opening not found.");

        return ApiResponse<JobOpeningDto>.Ok(new JobOpeningDto
        {
            Id = j.Id,
            Title = j.Title,
            DepartmentId = j.DepartmentId,
            DepartmentName = j.Department.Name,
            DesignationId = j.DesignationId,
            DesignationTitle = j.Designation?.Title,
            BranchId = j.BranchId,
            BranchName = j.Branch.Name,
            Vacancies = j.Vacancies,
            EmploymentType = j.EmploymentType,
            ExperienceMinYrs = j.ExperienceMinYrs,
            ExperienceMaxYrs = j.ExperienceMaxYrs,
            SalaryMin = j.SalaryMin,
            SalaryMax = j.SalaryMax,
            Description = j.Description,
            Requirements = j.Requirements,
            PostedDate = j.PostedDate,
            ClosingDate = j.ClosingDate,
            Status = j.Status,
            ApplicationsCount = j.Applications.Count(a => !a.IsDeleted),
            CreatedAt = j.CreatedAt
        });
    }

    public async Task<ApiResponse<JobOpeningDto>> CreateJobOpeningAsync(CreateJobOpeningRequest request, int? userId)
    {
        var entity = new JobOpening
        {
            Title = request.Title,
            DepartmentId = request.DepartmentId,
            DesignationId = request.DesignationId,
            BranchId = request.BranchId,
            Vacancies = Math.Max(1, request.Vacancies),
            EmploymentType = request.EmploymentType,
            ExperienceMinYrs = request.ExperienceMinYrs,
            ExperienceMaxYrs = request.ExperienceMaxYrs,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            Description = request.Description,
            Requirements = request.Requirements,
            PostedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ClosingDate = request.ClosingDate,
            Status = "Open",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        db.JobOpenings.Add(entity);
        await db.SaveChangesAsync();

        await auditService.LogAsync(AuditActions.Create, "JobOpenings", entity.Id.ToString(), null, entity);
        return await GetJobOpeningByIdAsync(entity.Id);
    }

    public async Task<ApiResponse<JobOpeningDto>> UpdateJobOpeningAsync(int id, UpdateJobOpeningRequest request, int? userId)
    {
        var entity = await db.JobOpenings.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) return ApiResponse<JobOpeningDto>.Fail("Job opening not found.");

        var old = new { entity.Title, entity.Status, entity.Vacancies };

        entity.Title = request.Title;
        entity.DepartmentId = request.DepartmentId;
        entity.DesignationId = request.DesignationId;
        entity.BranchId = request.BranchId;
        entity.Vacancies = request.Vacancies;
        entity.EmploymentType = request.EmploymentType;
        entity.ExperienceMinYrs = request.ExperienceMinYrs;
        entity.ExperienceMaxYrs = request.ExperienceMaxYrs;
        entity.SalaryMin = request.SalaryMin;
        entity.SalaryMax = request.SalaryMax;
        entity.Description = request.Description;
        entity.Requirements = request.Requirements;
        entity.ClosingDate = request.ClosingDate;
        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "JobOpenings", entity.Id.ToString(), old, entity);
        return await GetJobOpeningByIdAsync(entity.Id);
    }

    public async Task<ApiResponse> DeleteJobOpeningAsync(int id, int? userId)
    {
        var entity = await db.JobOpenings.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (entity == null) return ApiResponse.Fail("Job opening not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Delete, "JobOpenings", id.ToString(), null, new { IsDeleted = true });
        return ApiResponse.Ok("Job opening deleted successfully.");
    }

    public async Task<ApiResponse<PagedResult<CandidateDto>>> GetCandidatesAsync(PagedQuery query)
    {
        var q = db.Candidates.AsNoTracking().Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            q = q.Where(c => c.FirstName.ToLower().Contains(search) || c.LastName.ToLower().Contains(search) || c.Email.ToLower().Contains(search));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(c => c.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new CandidateDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                ResumeUrl = c.ResumeUrl,
                LinkedInUrl = c.LinkedInUrl,
                Source = c.Source,
                CreatedAt = c.CreatedAt,
                ApplicationsCount = c.Applications.Count(a => !a.IsDeleted)
            })
            .ToListAsync();

        return ApiResponse<PagedResult<CandidateDto>>.Ok(new PagedResult<CandidateDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<CandidateDto>> GetCandidateByIdAsync(int id)
    {
        var c = await db.Candidates.AsNoTracking().Include(x => x.Applications).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (c == null) return ApiResponse<CandidateDto>.Fail("Candidate not found.");

        return ApiResponse<CandidateDto>.Ok(new CandidateDto
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            Phone = c.Phone,
            ResumeUrl = c.ResumeUrl,
            LinkedInUrl = c.LinkedInUrl,
            Source = c.Source,
            CreatedAt = c.CreatedAt,
            ApplicationsCount = c.Applications.Count(a => !a.IsDeleted)
        });
    }

    public async Task<ApiResponse<CandidateDto>> CreateCandidateAsync(CreateCandidateRequest request, int? userId)
    {
        var exists = await db.Candidates.AnyAsync(c => c.Email == request.Email && !c.IsDeleted);
        if (exists) return ApiResponse<CandidateDto>.Fail("A candidate with this email already exists.");

        var candidate = new Candidate
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            FullName = $"{request.FirstName} {request.LastName}".Trim(),
            Email = request.Email,
            Phone = request.Phone,
            ResumeUrl = request.ResumeUrl,
            LinkedInUrl = request.LinkedInUrl,
            Source = request.Source ?? "Direct",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        db.Candidates.Add(candidate);
        await db.SaveChangesAsync();

        await auditService.LogAsync(AuditActions.Create, "Candidates", candidate.Id.ToString(), null, candidate);
        return await GetCandidateByIdAsync(candidate.Id);
    }

    public async Task<ApiResponse<List<ApplicationDto>>> GetApplicationsAsync(int? jobOpeningId = null, string? stage = null)
    {
        var q = db.Applications
            .AsNoTracking()
            .Include(a => a.Candidate)
            .Include(a => a.JobOpening)
            .Include(a => a.Interviews)
            .Where(a => !a.IsDeleted);

        if (jobOpeningId.HasValue)
            q = q.Where(a => a.JobOpeningId == jobOpeningId.Value);

        if (!string.IsNullOrWhiteSpace(stage))
            q = q.Where(a => a.Stage == stage);

        var list = await q
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ApplicationDto
            {
                Id = a.Id,
                JobOpeningId = a.JobOpeningId,
                JobOpeningTitle = a.JobOpening.Title,
                CandidateId = a.CandidateId,
                CandidateName = a.Candidate.FirstName + " " + a.Candidate.LastName,
                CandidateEmail = a.Candidate.Email,
                CandidatePhone = a.Candidate.Phone,
                ResumeUrl = a.Candidate.ResumeUrl,
                Stage = a.Stage,
                StageOrder = a.StageOrder,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                InterviewsCount = a.Interviews.Count(i => !i.IsDeleted)
            })
            .ToListAsync();

        return ApiResponse<List<ApplicationDto>>.Ok(list);
    }

    public async Task<ApiResponse<ApplicationDto>> CreateApplicationAsync(CreateApplicationRequest request, int? userId)
    {
        var exists = await db.Applications.AnyAsync(a => a.JobOpeningId == request.JobOpeningId && a.CandidateId == request.CandidateId && !a.IsDeleted);
        if (exists) return ApiResponse<ApplicationDto>.Fail("Candidate has already applied for this position.");

        var app = new Application
        {
            JobOpeningId = request.JobOpeningId,
            CandidateId = request.CandidateId,
            Stage = "Applied",
            StageOrder = 1,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        db.Applications.Add(app);
        await db.SaveChangesAsync();

        var created = await db.Applications
            .AsNoTracking()
            .Include(a => a.Candidate)
            .Include(a => a.JobOpening)
            .FirstAsync(a => a.Id == app.Id);

        return ApiResponse<ApplicationDto>.Ok(new ApplicationDto
        {
            Id = created.Id,
            JobOpeningId = created.JobOpeningId,
            JobOpeningTitle = created.JobOpening.Title,
            CandidateId = created.CandidateId,
            CandidateName = created.Candidate.FirstName + " " + created.Candidate.LastName,
            CandidateEmail = created.Candidate.Email,
            CandidatePhone = created.Candidate.Phone,
            ResumeUrl = created.Candidate.ResumeUrl,
            Stage = created.Stage,
            StageOrder = created.StageOrder,
            Notes = created.Notes,
            CreatedAt = created.CreatedAt
        });
    }

    public async Task<ApiResponse<ApplicationDto>> UpdateApplicationStageAsync(int id, UpdateApplicationStageRequest request, int? userId)
    {
        var app = await db.Applications
            .Include(a => a.Candidate)
            .Include(a => a.JobOpening)
            .Include(a => a.Interviews)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (app == null) return ApiResponse<ApplicationDto>.Fail("Application not found.");

        app.Stage = request.Stage;
        app.StageOrder = request.Stage switch
        {
            "Applied" => 1,
            "Shortlisted" => 2,
            "Interview" => 3,
            "Offered" => 4,
            "Hired" => 5,
            "Rejected" => 6,
            _ => 1
        };

        if (!string.IsNullOrWhiteSpace(request.Notes))
            app.Notes = request.Notes;

        app.UpdatedAt = DateTime.UtcNow;
        app.UpdatedBy = userId;

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Applications", app.Id.ToString(), null, new { app.Stage, app.StageOrder });

        return ApiResponse<ApplicationDto>.Ok(new ApplicationDto
        {
            Id = app.Id,
            JobOpeningId = app.JobOpeningId,
            JobOpeningTitle = app.JobOpening.Title,
            CandidateId = app.CandidateId,
            CandidateName = app.Candidate.FirstName + " " + app.Candidate.LastName,
            CandidateEmail = app.Candidate.Email,
            CandidatePhone = app.Candidate.Phone,
            ResumeUrl = app.Candidate.ResumeUrl,
            Stage = app.Stage,
            StageOrder = app.StageOrder,
            Notes = app.Notes,
            CreatedAt = app.CreatedAt,
            InterviewsCount = app.Interviews.Count(i => !i.IsDeleted)
        });
    }

    public async Task<ApiResponse<List<InterviewDto>>> GetInterviewsAsync(int? applicationId = null, int? interviewerId = null)
    {
        var q = db.Interviews
            .AsNoTracking()
            .Include(i => i.Application).ThenInclude(a => a.Candidate)
            .Include(i => i.Application).ThenInclude(a => a.JobOpening)
            .Include(i => i.Interviewer)
            .Where(i => !i.IsDeleted);

        if (applicationId.HasValue)
            q = q.Where(i => i.ApplicationId == applicationId.Value);

        if (interviewerId.HasValue)
            q = q.Where(i => i.InterviewerId == interviewerId.Value);

        var list = await q
            .OrderByDescending(i => i.ScheduledAt)
            .Select(i => new InterviewDto
            {
                Id = i.Id,
                ApplicationId = i.ApplicationId,
                CandidateName = i.Application.Candidate.FirstName + " " + i.Application.Candidate.LastName,
                JobTitle = i.Application.JobOpening.Title,
                InterviewerId = i.InterviewerId,
                InterviewerName = i.Interviewer.FirstName + " " + i.Interviewer.LastName,
                ScheduledAt = i.ScheduledAt,
                Mode = i.Mode,
                Location = i.Location,
                MeetingLink = i.MeetingLink,
                Feedback = i.Feedback,
                Score = i.Score,
                Result = i.Result,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<InterviewDto>>.Ok(list);
    }

    public async Task<ApiResponse<InterviewDto>> ScheduleInterviewAsync(ScheduleInterviewRequest request, int? userId)
    {
        var interview = new Interview
        {
            ApplicationId = request.ApplicationId,
            InterviewerId = request.InterviewerId,
            ScheduledAt = request.ScheduledAt,
            Mode = request.Mode,
            Location = request.Location,
            MeetingLink = request.MeetingLink,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        db.Interviews.Add(interview);

        // Update application stage to Interview
        var app = await db.Applications.FindAsync(request.ApplicationId);
        if (app != null)
        {
            app.Stage = "Interview";
            app.StageOrder = 3;
            app.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        var created = await db.Interviews
            .AsNoTracking()
            .Include(i => i.Application).ThenInclude(a => a.Candidate)
            .Include(i => i.Application).ThenInclude(a => a.JobOpening)
            .Include(i => i.Interviewer)
            .FirstAsync(i => i.Id == interview.Id);

        return ApiResponse<InterviewDto>.Ok(new InterviewDto
        {
            Id = created.Id,
            ApplicationId = created.ApplicationId,
            CandidateName = created.Application.Candidate.FirstName + " " + created.Application.Candidate.LastName,
            JobTitle = created.Application.JobOpening.Title,
            InterviewerId = created.InterviewerId,
            InterviewerName = created.Interviewer.FirstName + " " + created.Interviewer.LastName,
            ScheduledAt = created.ScheduledAt,
            Mode = created.Mode,
            Location = created.Location,
            MeetingLink = created.MeetingLink,
            CreatedAt = created.CreatedAt
        });
    }

    public async Task<ApiResponse<InterviewDto>> SubmitInterviewFeedbackAsync(int id, SubmitInterviewFeedbackRequest request, int? userId)
    {
        var interview = await db.Interviews
            .Include(i => i.Application).ThenInclude(a => a.Candidate)
            .Include(i => i.Application).ThenInclude(a => a.JobOpening)
            .Include(i => i.Interviewer)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

        if (interview == null) return ApiResponse<InterviewDto>.Fail("Interview not found.");

        interview.Feedback = request.Feedback;
        interview.Score = request.Score;
        interview.Result = request.Result;
        interview.UpdatedAt = DateTime.UtcNow;
        interview.UpdatedBy = userId;

        await db.SaveChangesAsync();

        return ApiResponse<InterviewDto>.Ok(new InterviewDto
        {
            Id = interview.Id,
            ApplicationId = interview.ApplicationId,
            CandidateName = interview.Application.Candidate.FirstName + " " + interview.Application.Candidate.LastName,
            JobTitle = interview.Application.JobOpening.Title,
            InterviewerId = interview.InterviewerId,
            InterviewerName = interview.Interviewer.FirstName + " " + interview.Interviewer.LastName,
            ScheduledAt = interview.ScheduledAt,
            Mode = interview.Mode,
            Location = interview.Location,
            MeetingLink = interview.MeetingLink,
            Feedback = interview.Feedback,
            Score = interview.Score,
            Result = interview.Result,
            CreatedAt = interview.CreatedAt
        });
    }

    public async Task<ApiResponse<List<OfferLetterDto>>> GetOfferLettersAsync(int? applicationId = null)
    {
        var q = db.OfferLetters
            .AsNoTracking()
            .Include(o => o.Application).ThenInclude(a => a.Candidate)
            .Include(o => o.Application).ThenInclude(a => a.JobOpening)
            .Where(o => !o.IsDeleted);

        if (applicationId.HasValue)
            q = q.Where(o => o.ApplicationId == applicationId.Value);

        var list = await q
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OfferLetterDto
            {
                Id = o.Id,
                ApplicationId = o.ApplicationId,
                CandidateName = o.Application.Candidate.FirstName + " " + o.Application.Candidate.LastName,
                JobTitle = o.Application.JobOpening.Title,
                OfferedSalary = o.OfferedSalary,
                JoiningDate = o.JoiningDate,
                ExpiryDate = o.ExpiryDate,
                FileUrl = o.FileUrl,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<OfferLetterDto>>.Ok(list);
    }

    public async Task<ApiResponse<OfferLetterDto>> CreateOfferLetterAsync(CreateOfferLetterRequest request, int? userId)
    {
        var offer = new OfferLetter
        {
            ApplicationId = request.ApplicationId,
            OfferedSalary = request.OfferedSalary,
            JoiningDate = request.JoiningDate,
            ExpiryDate = request.ExpiryDate,
            FileUrl = request.FileUrl,
            Status = "Sent",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        db.OfferLetters.Add(offer);

        var app = await db.Applications.FindAsync(request.ApplicationId);
        if (app != null)
        {
            app.Stage = "Offered";
            app.StageOrder = 4;
            app.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        var created = await db.OfferLetters
            .AsNoTracking()
            .Include(o => o.Application).ThenInclude(a => a.Candidate)
            .Include(o => o.Application).ThenInclude(a => a.JobOpening)
            .FirstAsync(o => o.Id == offer.Id);

        return ApiResponse<OfferLetterDto>.Ok(new OfferLetterDto
        {
            Id = created.Id,
            ApplicationId = created.ApplicationId,
            CandidateName = created.Application.Candidate.FirstName + " " + created.Application.Candidate.LastName,
            JobTitle = created.Application.JobOpening.Title,
            OfferedSalary = created.OfferedSalary,
            JoiningDate = created.JoiningDate,
            ExpiryDate = created.ExpiryDate,
            FileUrl = created.FileUrl,
            Status = created.Status,
            CreatedAt = created.CreatedAt
        });
    }

    public async Task<ApiResponse<OfferLetterDto>> UpdateOfferStatusAsync(int id, UpdateOfferStatusRequest request, int? userId)
    {
        var offer = await db.OfferLetters
            .Include(o => o.Application).ThenInclude(a => a.Candidate)
            .Include(o => o.Application).ThenInclude(a => a.JobOpening)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

        if (offer == null) return ApiResponse<OfferLetterDto>.Fail("Offer letter not found.");

        offer.Status = request.Status;
        offer.UpdatedAt = DateTime.UtcNow;
        offer.UpdatedBy = userId;

        if (request.Status == "Accepted")
        {
            offer.Application.Stage = "Hired";
            offer.Application.StageOrder = 5;
            offer.Application.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        return ApiResponse<OfferLetterDto>.Ok(new OfferLetterDto
        {
            Id = offer.Id,
            ApplicationId = offer.ApplicationId,
            CandidateName = offer.Application.Candidate.FirstName + " " + offer.Application.Candidate.LastName,
            JobTitle = offer.Application.JobOpening.Title,
            OfferedSalary = offer.OfferedSalary,
            JoiningDate = offer.JoiningDate,
            ExpiryDate = offer.ExpiryDate,
            FileUrl = offer.FileUrl,
            Status = offer.Status,
            CreatedAt = offer.CreatedAt
        });
    }
}
