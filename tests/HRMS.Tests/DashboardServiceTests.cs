using System;
using System.Threading.Tasks;
using HRMS.Models;
using HRMS.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HRMS.Tests;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetDashboardSummaryAsync_ShouldReturnWorkforceMetrics()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var logger = NullLogger<DashboardService>.Instance;
        var service = new DashboardService(db, logger);

        var dept = new Department { Name = "Technology", Code = "TECH", CreatedAt = DateTime.UtcNow };
        db.Departments.Add(dept);
        await db.SaveChangesAsync();

        var emp1 = TestHelpers.CreateSampleEmployee("EMP-001", "Alice", "Khan", deptId: dept.Id, status: "Active");
        emp1.Department = dept;
        var emp2 = TestHelpers.CreateSampleEmployee("EMP-002", "Bob", "Rahman", deptId: dept.Id, status: "Probation");
        emp2.Department = dept;
        db.Employees.AddRange(emp1, emp2);
        await db.SaveChangesAsync();

        // Act
        var res = await service.GetDashboardSummaryAsync();

        // Assert
        Assert.True(res.Success);
        Assert.NotNull(res.Data);
        Assert.Equal(2, res.Data.TotalEmployees);
        Assert.Equal(1, res.Data.ActiveEmployees);
        Assert.Equal(1, res.Data.OnProbationEmployees);
        Assert.NotEmpty(res.Data.DepartmentDistribution);
        Assert.Equal("Technology", res.Data.DepartmentDistribution[0].DepartmentName);
    }
}
