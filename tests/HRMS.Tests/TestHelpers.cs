using System;
using System.Threading;
using System.Threading.Tasks;
using HRMS.Data;
using HRMS.IServices;
using HRMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace HRMS.Tests;

public class FakeAuditService : IAuditService
{
    public Task LogAsync(string action, string? entityType = null, string? entityId = null,
        object? oldValues = null, object? newValues = null, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}

public class FakeCacheService : ICacheService
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, object> _cache = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(key, out var val) && val is T typed)
            return Task.FromResult<T?>(typed);
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        if (value is not null)
            _cache[key] = value;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var existing = await GetAsync<T>(key, ct);
        if (existing is not null) return existing;
        var created = await factory();
        await SetAsync(key, created, expiration, ct);
        return created;
    }
}

public static class TestHelpers
{
    public static HrmsDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<HrmsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new HrmsDbContext(options);
    }

    public static Employee CreateSampleEmployee(string code, string first, string last, int branchId = 1, int deptId = 1, int desigId = 1, string status = "Active")
    {
        return new Employee
        {
            EmployeeCode = code,
            FirstName = first,
            LastName = last,
            FullName = $"{first} {last}",
            Gender = "Male",
            DateOfBirth = new DateOnly(1995, 5, 20),
            WorkEmail = $"{first.ToLower()}.{last.ToLower()}@hrms.local",
            BranchId = branchId,
            DepartmentId = deptId,
            DesignationId = desigId,
            JoiningDate = new DateOnly(2023, 1, 15),
            EmploymentType = "Permanent",
            Status = status,
            RowVersion = new byte[] { 0, 0, 0, 1 },
            CreatedAt = DateTime.UtcNow
        };
    }
}
