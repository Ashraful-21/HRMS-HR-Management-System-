using System;
using System.Threading.Tasks;
using HRMS.Dtos.Common;
using HRMS.Dtos.Recruitment;
using HRMS.Models;
using HRMS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HRMS.Tests;

public class RecruitmentServiceTests
{
    [Fact]
    public async Task CreateJobOpeningAsync_ShouldCreateJobOpeningSuccessfully()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var audit = new FakeAuditService();
        var logger = NullLogger<RecruitmentService>.Instance;
        var service = new RecruitmentService(db, audit, logger);

        var dept = new Department { Name = "Engineering", Code = "ENG", CreatedAt = DateTime.UtcNow };
        var branch = new Branch { Name = "Dhaka HQ", Code = "DHK", CreatedAt = DateTime.UtcNow };
        db.Departments.Add(dept);
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        var request = new CreateJobOpeningRequest
        {
            Title = "Senior Backend Engineer (.NET)",
            DepartmentId = dept.Id,
            BranchId = branch.Id,
            Vacancies = 2,
            ExperienceMinYrs = 4,
            EmploymentType = "Full-Time",
            ClosingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1))
        };

        // Act
        var response = await service.CreateJobOpeningAsync(request, 1);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.Id > 0);
        Assert.Equal("Senior Backend Engineer (.NET)", response.Data.Title);
        Assert.Equal("Open", response.Data.Status);

        var savedInDb = await db.JobOpenings.FirstOrDefaultAsync(j => j.Id == response.Data.Id);
        Assert.NotNull(savedInDb);
        Assert.Equal(2, savedInDb.Vacancies);
    }

    [Fact]
    public async Task CreateCandidateAndApplication_ShouldTrackApplicationPipeline()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var audit = new FakeAuditService();
        var logger = NullLogger<RecruitmentService>.Instance;
        var service = new RecruitmentService(db, audit, logger);

        var dept = new Department { Name = "QA", Code = "QA01", CreatedAt = DateTime.UtcNow };
        var branch = new Branch { Name = "HQ", Code = "HQ01", CreatedAt = DateTime.UtcNow };
        db.Departments.Add(dept);
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        // Create opening
        var opening = new JobOpening
        {
            Title = "Lead QA Engineer",
            DepartmentId = dept.Id,
            BranchId = branch.Id,
            Vacancies = 1,
            EmploymentType = "Full-Time",
            PostedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };
        db.JobOpenings.Add(opening);
        await db.SaveChangesAsync();

        // 1. Create candidate
        var candidateReq = new CreateCandidateRequest
        {
            FirstName = "Anisul",
            LastName = "Islam",
            Email = "anisul.qa@example.com",
            Phone = "+8801811223344",
            ResumeUrl = "https://cdn.example.com/resumes/anisul.pdf",
            Source = "LinkedIn"
        };
        var candRes = await service.CreateCandidateAsync(candidateReq, 1);
        Assert.True(candRes.Success);
        Assert.NotNull(candRes.Data);

        // 2. Submit application
        var appReq = new CreateApplicationRequest
        {
            JobOpeningId = opening.Id,
            CandidateId = candRes.Data.Id,
            Notes = "Extensive experience with Playwright and xUnit."
        };
        var appRes = await service.CreateApplicationAsync(appReq, 1);
        Assert.True(appRes.Success);
        Assert.NotNull(appRes.Data);
        Assert.Equal("Applied", appRes.Data.Stage);

        // 3. Move stage to Screening
        var stageReq = new UpdateApplicationStageRequest
        {
            Stage = "Screening",
            Notes = "Resume verified and passed preliminary screening."
        };
        var updateRes = await service.UpdateApplicationStageAsync(appRes.Data.Id, stageReq, 1);
        Assert.True(updateRes.Success);
        Assert.Equal("Screening", updateRes.Data?.Stage);
    }
}
