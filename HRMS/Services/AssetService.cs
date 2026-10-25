using HRMS.Constants;
using HRMS.Data;
using HRMS.Dtos.Assets;
using HRMS.Dtos.Common;
using HRMS.IServices;
using HRMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Services;

public class AssetService(HrmsDbContext db, IAuditService auditService, ILogger<AssetService> logger) : IAssetService
{
    public async Task<ApiResponse<List<AssetCategoryDto>>> GetCategoriesAsync()
    {
        var list = await db.AssetCategories
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .Select(c => new AssetCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                AssetsCount = c.Assets.Count(a => !a.IsDeleted),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<AssetCategoryDto>>.Ok(list);
    }

    public async Task<ApiResponse<AssetCategoryDto>> CreateCategoryAsync(CreateAssetCategoryRequest request, int? userId)
    {
        var exists = await db.AssetCategories.AnyAsync(c => c.Code == request.Code && !c.IsDeleted);
        if (exists) return ApiResponse<AssetCategoryDto>.Fail("Category code already exists.");

        var cat = new AssetCategory
        {
            Name = request.Name,
            Code = request.Code.ToUpper().Trim(),
            CreatedAt = DateTime.UtcNow
        };

        db.AssetCategories.Add(cat);
        await db.SaveChangesAsync();

        return ApiResponse<AssetCategoryDto>.Ok(new AssetCategoryDto
        {
            Id = cat.Id,
            Name = cat.Name,
            Code = cat.Code,
            AssetsCount = 0,
            CreatedAt = cat.CreatedAt
        });
    }

    public async Task<ApiResponse<PagedResult<AssetDto>>> GetAssetsAsync(PagedQuery query, int? categoryId = null, string? status = null)
    {
        var q = db.Assets
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.AssetAssignments.Where(x => !x.IsReturned)).ThenInclude(x => x.Employee)
            .Where(a => !a.IsDeleted);

        if (categoryId.HasValue) q = q.Where(a => a.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(a => a.Status == status);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            q = q.Where(a => a.Name.ToLower().Contains(search) || a.AssetCode.ToLower().Contains(search) || (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(search)));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(a => a.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new AssetDto
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                Name = a.Name,
                CategoryId = a.CategoryId,
                CategoryName = a.Category.Name,
                SerialNumber = a.SerialNumber,
                PurchaseDate = a.PurchaseDate,
                PurchaseCost = a.PurchaseCost,
                Condition = a.Condition,
                Status = a.Status,
                Notes = a.Notes,
                CurrentAssigneeId = a.AssetAssignments.FirstOrDefault(x => !x.IsReturned) != null ? a.AssetAssignments.FirstOrDefault(x => !x.IsReturned)!.EmployeeId : null,
                CurrentAssigneeName = a.AssetAssignments.FirstOrDefault(x => !x.IsReturned) != null ? a.AssetAssignments.FirstOrDefault(x => !x.IsReturned)!.Employee.FirstName + " " + a.AssetAssignments.FirstOrDefault(x => !x.IsReturned)!.Employee.LastName : null,
                AssignedDate = a.AssetAssignments.FirstOrDefault(x => !x.IsReturned) != null ? a.AssetAssignments.FirstOrDefault(x => !x.IsReturned)!.AssignedDate : null,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<PagedResult<AssetDto>>.Ok(new PagedResult<AssetDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<AssetDto>> GetAssetByIdAsync(int id)
    {
        var a = await db.Assets
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.AssetAssignments.Where(y => !y.IsReturned)).ThenInclude(y => y.Employee)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (a == null) return ApiResponse<AssetDto>.Fail("Asset not found.");

        var activeAssign = a.AssetAssignments.FirstOrDefault(x => !x.IsReturned);

        return ApiResponse<AssetDto>.Ok(new AssetDto
        {
            Id = a.Id,
            AssetCode = a.AssetCode,
            Name = a.Name,
            CategoryId = a.CategoryId,
            CategoryName = a.Category.Name,
            SerialNumber = a.SerialNumber,
            PurchaseDate = a.PurchaseDate,
            PurchaseCost = a.PurchaseCost,
            Condition = a.Condition,
            Status = a.Status,
            Notes = a.Notes,
            CurrentAssigneeId = activeAssign?.EmployeeId,
            CurrentAssigneeName = activeAssign != null ? activeAssign.Employee.FirstName + " " + activeAssign.Employee.LastName : null,
            AssignedDate = activeAssign?.AssignedDate,
            CreatedAt = a.CreatedAt
        });
    }

    public async Task<ApiResponse<AssetDto>> CreateAssetAsync(CreateAssetRequest request, int? userId)
    {
        var exists = await db.Assets.AnyAsync(a => a.AssetCode == request.AssetCode && !a.IsDeleted);
        if (exists) return ApiResponse<AssetDto>.Fail("Asset code already exists.");

        var asset = new Asset
        {
            AssetCode = request.AssetCode.ToUpper().Trim(),
            Name = request.Name,
            CategoryId = request.CategoryId,
            SerialNumber = request.SerialNumber,
            PurchaseDate = request.PurchaseDate,
            PurchaseCost = request.PurchaseCost,
            Condition = request.Condition,
            Status = "Available",
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        db.Assets.Add(asset);
        await db.SaveChangesAsync();

        await auditService.LogAsync(AuditActions.Create, "Assets", asset.Id.ToString(), null, asset);
        return await GetAssetByIdAsync(asset.Id);
    }

    public async Task<ApiResponse<AssetDto>> UpdateAssetAsync(int id, UpdateAssetRequest request, int? userId)
    {
        var asset = await db.Assets.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        if (asset == null) return ApiResponse<AssetDto>.Fail("Asset not found.");

        asset.Name = request.Name;
        asset.CategoryId = request.CategoryId;
        asset.SerialNumber = request.SerialNumber;
        asset.PurchaseDate = request.PurchaseDate;
        asset.PurchaseCost = request.PurchaseCost;
        asset.Condition = request.Condition;
        asset.Status = request.Status;
        asset.Notes = request.Notes;
        asset.UpdatedAt = DateTime.UtcNow;
        asset.UpdatedBy = userId;

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Assets", asset.Id.ToString(), null, asset);
        return await GetAssetByIdAsync(asset.Id);
    }

    public async Task<ApiResponse> DeleteAssetAsync(int id, int? userId)
    {
        var asset = await db.Assets.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        if (asset == null) return ApiResponse.Fail("Asset not found.");

        asset.IsDeleted = true;
        asset.UpdatedAt = DateTime.UtcNow;
        asset.UpdatedBy = userId;

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Delete, "Assets", id.ToString(), null, new { IsDeleted = true });
        return ApiResponse.Ok("Asset deleted successfully.");
    }

    public async Task<ApiResponse<List<AssetAssignmentDto>>> GetAssignmentsAsync(int? assetId = null, int? employeeId = null)
    {
        var q = db.AssetAssignments
            .AsNoTracking()
            .Include(a => a.Asset)
            .Include(a => a.Employee)
            .AsQueryable();

        if (assetId.HasValue) q = q.Where(a => a.AssetId == assetId.Value);
        if (employeeId.HasValue) q = q.Where(a => a.EmployeeId == employeeId.Value);

        var list = await q
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AssetAssignmentDto
            {
                Id = a.Id,
                AssetId = a.AssetId,
                AssetName = a.Asset.Name,
                AssetCode = a.Asset.AssetCode,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.FirstName + " " + a.Employee.LastName,
                EmployeeCode = a.Employee.EmployeeCode,
                AssignedDate = a.AssignedDate,
                ReturnedDate = a.ReturnedDate,
                Condition = a.Condition,
                ReturnCondition = a.ReturnCondition,
                Notes = a.Notes,
                IsReturned = a.IsReturned,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<AssetAssignmentDto>>.Ok(list);
    }

    public async Task<ApiResponse<AssetAssignmentDto>> AssignAssetAsync(AssignAssetRequest request, int? userId)
    {
        var asset = await db.Assets.FirstOrDefaultAsync(a => a.Id == request.AssetId && !a.IsDeleted);
        if (asset == null) return ApiResponse<AssetAssignmentDto>.Fail("Asset not found.");

        if (asset.Status != "Available")
            return ApiResponse<AssetAssignmentDto>.Fail($"Asset cannot be assigned because its current status is '{asset.Status}'.");

        var assignment = new AssetAssignment
        {
            AssetId = request.AssetId,
            EmployeeId = request.EmployeeId,
            AssignedDate = request.AssignedDate,
            Condition = request.Condition,
            Notes = request.Notes,
            IsReturned = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        asset.Status = "Assigned";
        asset.UpdatedAt = DateTime.UtcNow;

        db.AssetAssignments.Add(assignment);
        await db.SaveChangesAsync();

        var created = await db.AssetAssignments
            .AsNoTracking()
            .Include(a => a.Asset)
            .Include(a => a.Employee)
            .FirstAsync(a => a.Id == assignment.Id);

        return ApiResponse<AssetAssignmentDto>.Ok(new AssetAssignmentDto
        {
            Id = created.Id,
            AssetId = created.AssetId,
            AssetName = created.Asset.Name,
            AssetCode = created.Asset.AssetCode,
            EmployeeId = created.EmployeeId,
            EmployeeName = created.Employee.FirstName + " " + created.Employee.LastName,
            EmployeeCode = created.Employee.EmployeeCode,
            AssignedDate = created.AssignedDate,
            Condition = created.Condition,
            Notes = created.Notes,
            IsReturned = false,
            CreatedAt = created.CreatedAt
        });
    }

    public async Task<ApiResponse<AssetAssignmentDto>> ReturnAssetAsync(int assignmentId, ReturnAssetRequest request, int? userId)
    {
        var assignment = await db.AssetAssignments
            .Include(a => a.Asset)
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (assignment == null) return ApiResponse<AssetAssignmentDto>.Fail("Asset assignment not found.");
        if (assignment.IsReturned) return ApiResponse<AssetAssignmentDto>.Fail("Asset has already been returned.");

        assignment.IsReturned = true;
        assignment.ReturnedDate = request.ReturnedDate;
        assignment.ReturnCondition = request.ReturnCondition;
        if (!string.IsNullOrWhiteSpace(request.Notes))
            assignment.Notes = (assignment.Notes + " | Return: " + request.Notes).Trim();
        assignment.UpdatedAt = DateTime.UtcNow;
        assignment.UpdatedBy = userId;

        assignment.Asset.Status = "Available";
        assignment.Asset.Condition = request.ReturnCondition;
        assignment.Asset.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return ApiResponse<AssetAssignmentDto>.Ok(new AssetAssignmentDto
        {
            Id = assignment.Id,
            AssetId = assignment.AssetId,
            AssetName = assignment.Asset.Name,
            AssetCode = assignment.Asset.AssetCode,
            EmployeeId = assignment.EmployeeId,
            EmployeeName = assignment.Employee.FirstName + " " + assignment.Employee.LastName,
            EmployeeCode = assignment.Employee.EmployeeCode,
            AssignedDate = assignment.AssignedDate,
            ReturnedDate = assignment.ReturnedDate,
            Condition = assignment.Condition,
            ReturnCondition = assignment.ReturnCondition,
            Notes = assignment.Notes,
            IsReturned = true,
            CreatedAt = assignment.CreatedAt
        });
    }
}
