using System;
using System.Threading.Tasks;
using HRMS.Dtos.Performance;
using HRMS.Models;
using HRMS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HRMS.Tests;

public class PerformanceServiceTests
{
    [Fact]
    public async Task CreateReviewCycleAsync_ShouldCreateActiveCycle()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var audit = new FakeAuditService();
        var logger = NullLogger<PerformanceService>.Instance;
        var service = new PerformanceService(db, audit, logger);

        var request = new CreateReviewCycleRequest
        {
            Name = "Q4 2026 Executive Review",
            Year = 2026,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1))
        };

        // Act
        var response = await service.CreateReviewCycleAsync(request, 1);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Q4 2026 Executive Review", response.Data.Name);
        Assert.Equal("Draft", response.Data.Status);
    }

    [Fact]
    public async Task AddGoalAsync_ShouldAppendGoalToPerformanceReview()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var audit = new FakeAuditService();
        var logger = NullLogger<PerformanceService>.Instance;
        var service = new PerformanceService(db, audit, logger);

        var cycle = new ReviewCycle
        {
            Name = "Annual 2026",
            Year = 2026,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31),
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };
        var emp = TestHelpers.CreateSampleEmployee("EMP-TST-01", "Mahmudul", "Hasan");
        db.ReviewCycles.Add(cycle);
        db.Employees.Add(emp);
        await db.SaveChangesAsync();

        var review = new PerformanceReview
        {
            ReviewCycleId = cycle.Id,
            EmployeeId = emp.Id,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow
        };
        db.PerformanceReviews.Add(review);
        await db.SaveChangesAsync();

        var goalRequest = new AddGoalRequest
        {
            GoalDescription = "Deliver HRMS Complete Production Release",
            Target = "10 enterprise modules, unit tests, and full glassmorphic UI.",
            Weight = 40,
            SelfScore = 95
        };

        // Act
        var result = await service.AddGoalAsync(review.Id, goalRequest);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Deliver HRMS Complete Production Release", result.Data.GoalDescription);
        Assert.Equal(40, result.Data.Weight);

        var savedGoal = await db.PerformanceGoals.FirstOrDefaultAsync(g => g.ReviewId == review.Id);
        Assert.NotNull(savedGoal);
    }
}
