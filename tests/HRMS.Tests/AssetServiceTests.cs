using System;
using System.Threading.Tasks;
using HRMS.Dtos.Assets;
using HRMS.Models;
using HRMS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HRMS.Tests;

public class AssetServiceTests
{
    [Fact]
    public async Task CreateAssetAndAssignAsync_ShouldUpdateAssetStatusAndRecordAssignment()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var audit = new FakeAuditService();
        var logger = NullLogger<AssetService>.Instance;
        var service = new AssetService(db, audit, logger);

        // 1. Create Category
        var cat = new AssetCategory
        {
            Name = "Laptops & Notebooks",
            Code = "CAT-LAPTOP",
            CreatedAt = DateTime.UtcNow
        };
        db.AssetCategories.Add(cat);

        // 2. Create Employee
        var emp = TestHelpers.CreateSampleEmployee("EMP-AST-01", "Nusrat", "Jahan");
        db.Employees.Add(emp);
        await db.SaveChangesAsync();

        // 3. Create Asset
        var createAssetReq = new CreateAssetRequest
        {
            AssetCode = "AST-MBP-14",
            Name = "MacBook Pro 14 M3",
            CategoryId = cat.Id,
            SerialNumber = "SN-APPLE-9988",
            PurchaseDate = new DateOnly(2026, 1, 10),
            PurchaseCost = 2499.00m,
            Condition = "Good"
        };
        var assetRes = await service.CreateAssetAsync(createAssetReq, 1);
        Assert.True(assetRes.Success);
        Assert.NotNull(assetRes.Data);
        Assert.Equal("Available", assetRes.Data.Status);

        // 4. Assign Asset to Employee
        var assignReq = new AssignAssetRequest
        {
            AssetId = assetRes.Data.Id,
            EmployeeId = emp.Id,
            AssignedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Condition = "Good",
            Notes = "Standard engineering allocation."
        };
        var assignRes = await service.AssignAssetAsync(assignReq, 1);
        Assert.True(assignRes.Success);
        Assert.NotNull(assignRes.Data);
        Assert.False(assignRes.Data.IsReturned);

        // Verify in DB
        var updatedAsset = await db.Assets.FindAsync(assetRes.Data.Id);
        Assert.NotNull(updatedAsset);
        Assert.Equal("Assigned", updatedAsset.Status);

        // 5. Return Asset
        var returnReq = new ReturnAssetRequest
        {
            ReturnedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
            ReturnCondition = "Fair",
            Notes = "Returned upon role transition."
        };
        var returnRes = await service.ReturnAssetAsync(assignRes.Data.Id, returnReq, 1);
        Assert.True(returnRes.Success);
        Assert.True(returnRes.Data?.IsReturned);

        var finalAsset = await db.Assets.FindAsync(assetRes.Data.Id);
        Assert.NotNull(finalAsset);
        Assert.Equal("Available", finalAsset.Status);
    }
}
