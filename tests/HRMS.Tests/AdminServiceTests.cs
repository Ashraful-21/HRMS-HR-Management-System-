using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.Dtos.Common;
using HRMS.Dtos.Users;
using HRMS.Models;
using HRMS.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HRMS.Tests;

public class AdminServiceTests
{
    [Fact]
    public async Task CreateUserAndAssignRoles_ShouldPersistUser()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var audit = new FakeAuditService();
        var cache = new FakeCacheService();
        var logger = NullLogger<AdminService>.Instance;
        var service = new AdminService(db, audit, cache, logger);

        var role = new Role
        {
            Name = "HR_Specialist",
            Description = "HR team member with recruitment and onboarding access"
        };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var request = new CreateUserRequest
        {
            Username = "hrspecialist",
            Email = "hr.specialist@hrms.local",
            Password = "SecurePassword@123",
            Roles = new List<string> { role.Name }
        };

        // Act
        var res = await service.CreateUserAsync(request, 1);

        // Assert
        Assert.True(res.Success);
        Assert.NotNull(res.Data);
        Assert.Equal("hrspecialist", res.Data.Username);
        Assert.Contains("HR_Specialist", res.Data.Roles);

        var userInDb = await db.Users.FirstOrDefaultAsync(u => u.Username == "hrspecialist");
        Assert.NotNull(userInDb);
        
        var hasher = new PasswordHasher<User>();
        var verifyResult = hasher.VerifyHashedPassword(userInDb, userInDb.PasswordHash, "SecurePassword@123");
        Assert.Equal(PasswordVerificationResult.Success, verifyResult);
    }

    [Fact]
    public async Task UpdateSettingAsync_ShouldUpdateSystemConfiguration()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var audit = new FakeAuditService();
        var cache = new FakeCacheService();
        var logger = NullLogger<AdminService>.Instance;
        var service = new AdminService(db, audit, cache, logger);

        var setting = new SystemSetting
        {
            Key = "Company.WorkDaysPerWeek",
            Value = "5",
            DataType = "Number",
            Category = "Attendance",
            Description = "Standard working days per week",
            IsPublic = true
        };
        db.SystemSettings.Add(setting);
        await db.SaveChangesAsync();

        var updateReq = new UpdateSettingRequest
        {
            Key = "Company.WorkDaysPerWeek",
            Value = "6"
        };

        // Act
        var res = await service.UpdateSettingAsync(updateReq, 1);

        // Assert
        Assert.True(res.Success);
        Assert.Equal("6", res.Data?.Value);

        var inDb = await db.SystemSettings.FirstAsync(s => s.Key == "Company.WorkDaysPerWeek");
        Assert.Equal("6", inDb.Value);
    }

    [Fact]
    public async Task GetSettingsAsync_ShouldServeFromCacheAndInvalidateOnUpdate()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var audit = new FakeAuditService();
        var cache = new FakeCacheService();
        var logger = NullLogger<AdminService>.Instance;
        var service = new AdminService(db, audit, cache, logger);

        db.SystemSettings.Add(new SystemSetting
        {
            Key = "Test.Key",
            Value = "InitialValue",
            DataType = "String",
            Category = "General"
        });
        await db.SaveChangesAsync();

        // Act 1: Initial fetch populates cache
        var initial = await service.GetSettingsAsync(null);
        Assert.True(initial.Success);
        Assert.Contains(initial.Data!, s => s.Key == "Test.Key" && s.Value == "InitialValue");

        // Act 2: Verify it is in cache
        var cached = await cache.GetAsync<ApiResponse<List<SystemSettingDto>>>("admin:settings:all");
        Assert.NotNull(cached);
        Assert.True(cached.Success);

        // Act 3: Update setting should invalidate cache
        await service.UpdateSettingAsync(new UpdateSettingRequest { Key = "Test.Key", Value = "UpdatedValue" }, 1);
        var afterInvalidate = await cache.GetAsync<ApiResponse<List<SystemSettingDto>>>("admin:settings:all");
        Assert.Null(afterInvalidate);

        // Act 4: Re-fetch gets updated value and re-populates cache
        var reFetched = await service.GetSettingsAsync(null);
        Assert.Contains(reFetched.Data!, s => s.Key == "Test.Key" && s.Value == "UpdatedValue");
    }
}
