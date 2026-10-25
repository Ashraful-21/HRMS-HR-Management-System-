using HRMS.Constants;
using HRMS.Dtos.Common;
using HRMS.Dtos.Organization;
using HRMS.IRepositories;
using HRMS.IServices;
using HRMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Services;

public class OrganizationService(
    IUnitOfWork uow,
    IAuditService auditService,
    ICacheService cache,
    ILogger<OrganizationService> logger) : IOrganizationService
{
    // ==========================================
    // BRANCHES
    // ==========================================
    public async Task<ApiResponse<PagedResult<BranchDto>>> GetBranchesAsync(PagedQuery query)
    {
        var q = uow.Repository<Branch>().Query();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            q = q.Where(b => b.Name.ToLower().Contains(search) || b.Code.ToLower().Contains(search) || (b.City != null && b.City.ToLower().Contains(search)));
        }

        var total = await q.CountAsync();

        var items = await q
            .OrderByDescending(b => b.IsHeadOffice)
            .ThenBy(b => b.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(b => new BranchDto
            {
                Id = b.Id,
                Name = b.Name,
                Code = b.Code,
                Address = b.Address,
                City = b.City,
                Country = b.Country,
                Phone = b.Phone,
                Email = b.Email,
                IsHeadOffice = b.IsHeadOffice,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt,
                DepartmentCount = b.Departments.Count,
                EmployeeCount = b.Employees.Count
            })
            .ToListAsync();

        return ApiResponse<PagedResult<BranchDto>>.Ok(new PagedResult<BranchDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public async Task<ApiResponse<List<BranchDto>>> GetAllBranchesAsync()
    {
        return await cache.GetOrCreateAsync("org:branches:all", async () =>
        {
            var items = await uow.Repository<Branch>().Query()
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .Select(b => new BranchDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Code = b.Code,
                    City = b.City,
                    Country = b.Country,
                    IsHeadOffice = b.IsHeadOffice,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<BranchDto>>.Ok(items);
        }, TimeSpan.FromHours(1));
    }

    public async Task<ApiResponse<BranchDto>> GetBranchByIdAsync(int id)
    {
        return await cache.GetOrCreateAsync($"org:branch:{id}", async () =>
        {
            var b = await uow.Repository<Branch>().Query()
                .Include(x => x.Departments)
                .Include(x => x.Employees)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (b == null)
                return ApiResponse<BranchDto>.Fail("Branch not found.");

            return ApiResponse<BranchDto>.Ok(new BranchDto
            {
                Id = b.Id,
                Name = b.Name,
                Code = b.Code,
                Address = b.Address,
                City = b.City,
                Country = b.Country,
                Phone = b.Phone,
                Email = b.Email,
                IsHeadOffice = b.IsHeadOffice,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt,
                DepartmentCount = b.Departments.Count,
                EmployeeCount = b.Employees.Count
            });
        }, TimeSpan.FromHours(1));
    }

    public async Task<ApiResponse<BranchDto>> CreateBranchAsync(CreateBranchRequest req, int? userId)
    {
        var exists = await uow.Repository<Branch>().Query().AnyAsync(b => b.Code.ToLower() == req.Code.Trim().ToLower());
        if (exists)
            return ApiResponse<BranchDto>.Fail($"Branch code '{req.Code}' is already in use.");

        if (req.IsHeadOffice)
        {
            var headOffices = await uow.Repository<Branch>().Query().Where(b => b.IsHeadOffice).ToListAsync();
            foreach (var ho in headOffices)
            {
                ho.IsHeadOffice = false;
            }
        }

        var branch = new Branch
        {
            Name = req.Name.Trim(),
            Code = req.Code.Trim().ToUpperInvariant(),
            Address = req.Address,
            City = req.City,
            Country = req.Country,
            Phone = req.Phone,
            Email = req.Email,
            IsHeadOffice = req.IsHeadOffice,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await uow.Repository<Branch>().AddAsync(branch);
        await uow.SaveChangesAsync();

        await auditService.LogAsync(AuditActions.Create, "Branches", branch.Id.ToString(), null, branch);

        // Invalidate branch cache
        await cache.RemoveAsync("org:branches:all");

        return await GetBranchByIdAsync(branch.Id);
    }

    public async Task<ApiResponse<BranchDto>> UpdateBranchAsync(int id, UpdateBranchRequest req, int? userId)
    {
        var branch = await uow.Repository<Branch>().GetByIdAsync(id);
        if (branch == null)
            return ApiResponse<BranchDto>.Fail("Branch not found.");

        var codeExists = await uow.Repository<Branch>().Query()
            .AnyAsync(b => b.Id != id && b.Code.ToLower() == req.Code.Trim().ToLower());
        if (codeExists)
            return ApiResponse<BranchDto>.Fail($"Branch code '{req.Code}' is already in use.");

        if (req.IsHeadOffice && !branch.IsHeadOffice)
        {
            var otherHeadOffices = await uow.Repository<Branch>().Query().Where(b => b.Id != id && b.IsHeadOffice).ToListAsync();
            foreach (var ho in otherHeadOffices)
            {
                ho.IsHeadOffice = false;
            }
        }

        branch.Name = req.Name.Trim();
        branch.Code = req.Code.Trim().ToUpperInvariant();
        branch.Address = req.Address;
        branch.City = req.City;
        branch.Country = req.Country;
        branch.Phone = req.Phone;
        branch.Email = req.Email;
        branch.IsHeadOffice = req.IsHeadOffice;
        branch.IsActive = req.IsActive;
        branch.UpdatedAt = DateTime.UtcNow;
        branch.UpdatedBy = userId;

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Branches", branch.Id.ToString(), null, branch);

        // Invalidate branch cache
        await cache.RemoveAsync("org:branches:all");
        await cache.RemoveAsync($"org:branch:{id}");

        return await GetBranchByIdAsync(branch.Id);
    }

    public async Task<ApiResponse> DeleteBranchAsync(int id, int? userId)
    {
        var branch = await uow.Repository<Branch>().GetByIdAsync(id);
        if (branch == null)
            return ApiResponse.Fail("Branch not found.");

        var hasEmployees = await uow.Repository<Employee>().Query().AnyAsync(e => e.BranchId == id);
        if (hasEmployees)
            return ApiResponse.Fail("Cannot delete branch with assigned employees.");

        branch.IsDeleted = true;
        branch.UpdatedAt = DateTime.UtcNow;
        branch.UpdatedBy = userId;

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Delete, "Branches", branch.Id.ToString(), null, null);

        // Invalidate branch cache
        await cache.RemoveAsync("org:branches:all");
        await cache.RemoveAsync($"org:branch:{id}");

        return ApiResponse.Ok("Branch deleted successfully.");
    }

    // ==========================================
    // DEPARTMENTS
    // ==========================================
    public async Task<ApiResponse<List<DepartmentDto>>> GetDepartmentsAsync(int? branchId)
    {
        var cacheKey = $"org:depts:{branchId ?? 0}";
        return await cache.GetOrCreateAsync(cacheKey, async () =>
        {
            var q = uow.Repository<Department>().Query();
            if (branchId.HasValue)
                q = q.Where(d => d.BranchId == branchId.Value);

            var items = await q
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Code = d.Code,
                    BranchId = d.BranchId,
                    BranchName = d.Branch.Name,
                    ParentId = d.ParentId,
                    ParentName = d.Parent != null ? d.Parent.Name : null,
                    HeadEmployeeId = d.HeadEmployeeId,
                    HeadEmployeeName = d.HeadEmployee != null ? d.HeadEmployee.FullName : null,
                    Description = d.Description,
                    IsActive = d.IsActive,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<DepartmentDto>>.Ok(items);
        }, TimeSpan.FromHours(1));
    }

    public async Task<ApiResponse<List<DepartmentTreeNodeDto>>> GetDepartmentTreeAsync(int? branchId)
    {
        var cacheKey = $"org:depts:tree:{branchId ?? 0}";
        return await cache.GetOrCreateAsync(cacheKey, async () =>
        {
            var q = uow.Repository<Department>().Query();
            if (branchId.HasValue)
                q = q.Where(d => d.BranchId == branchId.Value);

            var list = await q.Select(d => new
            {
                d.Id,
                d.Name,
                d.Code,
                d.BranchId,
                d.ParentId,
                HeadEmployeeName = d.HeadEmployee != null ? d.HeadEmployee.FullName : null
            }).ToListAsync();

            var nodes = list.Select(d => new DepartmentTreeNodeDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                BranchId = d.BranchId,
                HeadEmployeeName = d.HeadEmployeeName
            }).ToDictionary(d => d.Id);

            var roots = new List<DepartmentTreeNodeDto>();

            foreach (var item in list)
            {
                if (item.ParentId.HasValue && nodes.TryGetValue(item.ParentId.Value, out var parentNode))
                {
                    parentNode.Children.Add(nodes[item.Id]);
                }
                else
                {
                    roots.Add(nodes[item.Id]);
                }
            }

            return ApiResponse<List<DepartmentTreeNodeDto>>.Ok(roots);
        }, TimeSpan.FromHours(1));
    }

    public async Task<ApiResponse<DepartmentDto>> GetDepartmentByIdAsync(int id)
    {
        var d = await uow.Repository<Department>().Query()
            .Include(x => x.Branch)
            .Include(x => x.Parent)
            .Include(x => x.HeadEmployee)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (d == null)
            return ApiResponse<DepartmentDto>.Fail("Department not found.");

        return ApiResponse<DepartmentDto>.Ok(new DepartmentDto
        {
            Id = d.Id,
            Name = d.Name,
            Code = d.Code,
            BranchId = d.BranchId,
            BranchName = d.Branch.Name,
            ParentId = d.ParentId,
            ParentName = d.Parent?.Name,
            HeadEmployeeId = d.HeadEmployeeId,
            HeadEmployeeName = d.HeadEmployee?.FullName,
            Description = d.Description,
            IsActive = d.IsActive,
            CreatedAt = d.CreatedAt
        });
    }

    public async Task<ApiResponse<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentRequest req, int? userId)
    {
        var exists = await uow.Repository<Department>().Query().AnyAsync(d => d.Code.ToLower() == req.Code.Trim().ToLower());
        if (exists)
            return ApiResponse<DepartmentDto>.Fail($"Department code '{req.Code}' already exists.");

        var branchExists = await uow.Repository<Branch>().ExistsAsync(req.BranchId);
        if (!branchExists)
            return ApiResponse<DepartmentDto>.Fail("Specified Branch does not exist.");

        var dept = new Department
        {
            Name = req.Name.Trim(),
            Code = req.Code.Trim().ToUpperInvariant(),
            BranchId = req.BranchId,
            ParentId = req.ParentId,
            HeadEmployeeId = req.HeadEmployeeId,
            Description = req.Description,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await uow.Repository<Department>().AddAsync(dept);
        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Create, "Departments", dept.Id.ToString(), null, dept);

        // Invalidate department caches
        await cache.RemoveAsync($"org:depts:{dept.BranchId}");
        await cache.RemoveAsync("org:depts:0");
        await cache.RemoveAsync($"org:depts:tree:{dept.BranchId}");
        await cache.RemoveAsync("org:depts:tree:0");

        return await GetDepartmentByIdAsync(dept.Id);
    }

    public async Task<ApiResponse<DepartmentDto>> UpdateDepartmentAsync(int id, UpdateDepartmentRequest req, int? userId)
    {
        var dept = await uow.Repository<Department>().GetByIdAsync(id);
        if (dept == null)
            return ApiResponse<DepartmentDto>.Fail("Department not found.");

        if (req.ParentId == id)
            return ApiResponse<DepartmentDto>.Fail("A department cannot be its own parent.");

        var codeExists = await uow.Repository<Department>().Query()
            .AnyAsync(d => d.Id != id && d.Code.ToLower() == req.Code.Trim().ToLower());
        if (codeExists)
            return ApiResponse<DepartmentDto>.Fail($"Department code '{req.Code}' already exists.");

        dept.Name = req.Name.Trim();
        dept.Code = req.Code.Trim().ToUpperInvariant();
        dept.BranchId = req.BranchId;
        dept.ParentId = req.ParentId;
        dept.HeadEmployeeId = req.HeadEmployeeId;
        dept.Description = req.Description;
        dept.IsActive = req.IsActive;
        dept.UpdatedAt = DateTime.UtcNow;
        dept.UpdatedBy = userId;

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Departments", dept.Id.ToString(), null, dept);

        // Invalidate department caches
        await cache.RemoveAsync($"org:depts:{dept.BranchId}");
        await cache.RemoveAsync("org:depts:0");
        await cache.RemoveAsync($"org:depts:tree:{dept.BranchId}");
        await cache.RemoveAsync("org:depts:tree:0");

        return await GetDepartmentByIdAsync(dept.Id);
    }

    public async Task<ApiResponse> DeleteDepartmentAsync(int id, int? userId)
    {
        var dept = await uow.Repository<Department>().GetByIdAsync(id);
        if (dept == null)
            return ApiResponse.Fail("Department not found.");

        var hasChildren = await uow.Repository<Department>().Query().AnyAsync(d => d.ParentId == id);
        if (hasChildren)
            return ApiResponse.Fail("Cannot delete department with active sub-departments.");

        var hasDesignations = await uow.Repository<Designation>().Query().AnyAsync(d => d.DepartmentId == id);
        if (hasDesignations)
            return ApiResponse.Fail("Cannot delete department with assigned designations.");

        dept.IsDeleted = true;
        dept.UpdatedAt = DateTime.UtcNow;
        dept.UpdatedBy = userId;

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Delete, "Departments", dept.Id.ToString(), null, null);

        // Invalidate department caches
        await cache.RemoveAsync($"org:depts:{dept.BranchId}");
        await cache.RemoveAsync("org:depts:0");
        await cache.RemoveAsync($"org:depts:tree:{dept.BranchId}");
        await cache.RemoveAsync("org:depts:tree:0");

        return ApiResponse.Ok("Department deleted successfully.");
    }

    // ==========================================
    // DESIGNATIONS
    // ==========================================
    public async Task<ApiResponse<List<DesignationDto>>> GetDesignationsAsync(int? departmentId)
    {
        var cacheKey = $"org:desigs:{departmentId ?? 0}";
        return await cache.GetOrCreateAsync(cacheKey, async () =>
        {
            var q = uow.Repository<Designation>().Query();
            if (departmentId.HasValue)
                q = q.Where(d => d.DepartmentId == departmentId.Value);

            var items = await q
                .OrderBy(d => d.Title)
                .Select(d => new DesignationDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    Code = d.Code,
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.Department.Name,
                    IsActive = d.IsActive,
                    CreatedAt = d.CreatedAt,
                    EmployeeCount = d.Employees.Count
                })
                .ToListAsync();

            return ApiResponse<List<DesignationDto>>.Ok(items);
        }, TimeSpan.FromHours(1));
    }

    public async Task<ApiResponse<DesignationDto>> GetDesignationByIdAsync(int id)
    {
        var d = await uow.Repository<Designation>().Query()
            .Include(x => x.Department)
            .Include(x => x.Employees)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (d == null)
            return ApiResponse<DesignationDto>.Fail("Designation not found.");

        return ApiResponse<DesignationDto>.Ok(new DesignationDto
        {
            Id = d.Id,
            Title = d.Title,
            Code = d.Code,
            DepartmentId = d.DepartmentId,
            DepartmentName = d.Department.Name,
            IsActive = d.IsActive,
            CreatedAt = d.CreatedAt,
            EmployeeCount = d.Employees.Count
        });
    }

    public async Task<ApiResponse<DesignationDto>> CreateDesignationAsync(CreateDesignationRequest req, int? userId)
    {
        var exists = await uow.Repository<Designation>().Query().AnyAsync(d => d.Code.ToLower() == req.Code.Trim().ToLower());
        if (exists)
            return ApiResponse<DesignationDto>.Fail($"Designation code '{req.Code}' already exists.");

        var dept = new Designation
        {
            Title = req.Title.Trim(),
            Code = req.Code.Trim().ToUpperInvariant(),
            DepartmentId = req.DepartmentId,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await uow.Repository<Designation>().AddAsync(dept);
        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Create, "Designations", dept.Id.ToString(), null, dept);

        // Invalidate designation caches
        await cache.RemoveAsync($"org:desigs:{dept.DepartmentId}");
        await cache.RemoveAsync("org:desigs:0");

        return await GetDesignationByIdAsync(dept.Id);
    }

    public async Task<ApiResponse<DesignationDto>> UpdateDesignationAsync(int id, UpdateDesignationRequest req, int? userId)
    {
        var desig = await uow.Repository<Designation>().GetByIdAsync(id);
        if (desig == null)
            return ApiResponse<DesignationDto>.Fail("Designation not found.");

        desig.Title = req.Title.Trim();
        desig.Code = req.Code.Trim().ToUpperInvariant();
        desig.DepartmentId = req.DepartmentId;
        desig.IsActive = req.IsActive;
        desig.UpdatedAt = DateTime.UtcNow;
        desig.UpdatedBy = userId;

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Designations", desig.Id.ToString(), null, desig);

        // Invalidate designation caches
        await cache.RemoveAsync($"org:desigs:{desig.DepartmentId}");
        await cache.RemoveAsync("org:desigs:0");

        return await GetDesignationByIdAsync(desig.Id);
    }

    public async Task<ApiResponse> DeleteDesignationAsync(int id, int? userId)
    {
        var desig = await uow.Repository<Designation>().GetByIdAsync(id);
        if (desig == null)
            return ApiResponse.Fail("Designation not found.");

        var hasEmployees = await uow.Repository<Employee>().Query().AnyAsync(e => e.DesignationId == id);
        if (hasEmployees)
            return ApiResponse.Fail("Cannot delete designation assigned to active employees.");

        desig.IsDeleted = true;
        desig.UpdatedAt = DateTime.UtcNow;
        desig.UpdatedBy = userId;

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Delete, "Designations", desig.Id.ToString(), null, null);

        // Invalidate designation caches
        await cache.RemoveAsync($"org:desigs:{desig.DepartmentId}");
        await cache.RemoveAsync("org:desigs:0");

        return ApiResponse.Ok("Designation deleted successfully.");
    }

    // ==========================================
    // GRADES
    // ==========================================
    public async Task<ApiResponse<List<GradeDto>>> GetGradesAsync()
    {
        return await cache.GetOrCreateAsync("org:grades:all", async () =>
        {
            var items = await uow.Repository<Grade>().Query()
                .OrderBy(g => g.Name)
                .Select(g => new GradeDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Code = g.Code,
                    MinSalary = g.MinSalary,
                    MaxSalary = g.MaxSalary,
                    Description = g.Description,
                    IsActive = g.IsActive,
                    CreatedAt = g.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<GradeDto>>.Ok(items);
        }, TimeSpan.FromHours(1));
    }

    public async Task<ApiResponse<GradeDto>> GetGradeByIdAsync(int id)
    {
        var g = await uow.Repository<Grade>().GetByIdAsync(id);
        if (g == null)
            return ApiResponse<GradeDto>.Fail("Grade not found.");

        return ApiResponse<GradeDto>.Ok(new GradeDto
        {
            Id = g.Id,
            Name = g.Name,
            Code = g.Code,
            MinSalary = g.MinSalary,
            MaxSalary = g.MaxSalary,
            Description = g.Description,
            IsActive = g.IsActive,
            CreatedAt = g.CreatedAt
        });
    }

    public async Task<ApiResponse<GradeDto>> CreateGradeAsync(CreateGradeRequest req, int? userId)
    {
        var exists = await uow.Repository<Grade>().Query().AnyAsync(g => g.Code.ToLower() == req.Code.Trim().ToLower());
        if (exists)
            return ApiResponse<GradeDto>.Fail($"Grade code '{req.Code}' already exists.");

        var grade = new Grade
        {
            Name = req.Name.Trim(),
            Code = req.Code.Trim().ToUpperInvariant(),
            MinSalary = req.MinSalary,
            MaxSalary = req.MaxSalary,
            Description = req.Description,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await uow.Repository<Grade>().AddAsync(grade);
        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Create, "Grades", grade.Id.ToString(), null, grade);

        await cache.RemoveAsync("org:grades:all");

        return await GetGradeByIdAsync(grade.Id);
    }

    public async Task<ApiResponse<GradeDto>> UpdateGradeAsync(int id, UpdateGradeRequest req, int? userId)
    {
        var grade = await uow.Repository<Grade>().GetByIdAsync(id);
        if (grade == null)
            return ApiResponse<GradeDto>.Fail("Grade not found.");

        grade.Name = req.Name.Trim();
        grade.Code = req.Code.Trim().ToUpperInvariant();
        grade.MinSalary = req.MinSalary;
        grade.MaxSalary = req.MaxSalary;
        grade.Description = req.Description;
        grade.IsActive = req.IsActive;
        grade.UpdatedAt = DateTime.UtcNow;
        grade.UpdatedBy = userId;

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Grades", grade.Id.ToString(), null, grade);

        await cache.RemoveAsync("org:grades:all");

        return await GetGradeByIdAsync(grade.Id);
    }

    public async Task<ApiResponse> DeleteGradeAsync(int id, int? userId)
    {
        var grade = await uow.Repository<Grade>().GetByIdAsync(id);
        if (grade == null)
            return ApiResponse.Fail("Grade not found.");

        grade.IsDeleted = true;
        grade.UpdatedAt = DateTime.UtcNow;
        grade.UpdatedBy = userId;

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Delete, "Grades", grade.Id.ToString(), null, null);

        await cache.RemoveAsync("org:grades:all");

        return ApiResponse.Ok("Grade deleted successfully.");
    }

    // ==========================================
    // SHIFTS
    // ==========================================
    public async Task<ApiResponse<List<ShiftDto>>> GetShiftsAsync()
    {
        return await cache.GetOrCreateAsync("org:shifts:all", async () =>
        {
            var items = await uow.Repository<Shift>().Query()
                .OrderBy(s => s.Name)
                .Select(s => new ShiftDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    GraceMinutes = s.GraceMinutes,
                    IsNightShift = s.IsNightShift,
                    WorkingHours = s.WorkingHours,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<ShiftDto>>.Ok(items);
        }, TimeSpan.FromHours(1));
    }

    public async Task<ApiResponse<ShiftDto>> GetShiftByIdAsync(int id)
    {
        var s = await uow.Repository<Shift>().GetByIdAsync(id);
        if (s == null)
            return ApiResponse<ShiftDto>.Fail("Shift not found.");

        return ApiResponse<ShiftDto>.Ok(new ShiftDto
        {
            Id = s.Id,
            Name = s.Name,
            Code = s.Code,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            GraceMinutes = s.GraceMinutes,
            IsNightShift = s.IsNightShift,
            WorkingHours = s.WorkingHours,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt
        });
    }

    public async Task<ApiResponse<ShiftDto>> CreateShiftAsync(CreateShiftRequest req, int? userId)
    {
        var exists = await uow.Repository<Shift>().Query().AnyAsync(s => s.Code.ToLower() == req.Code.Trim().ToLower());
        if (exists)
            return ApiResponse<ShiftDto>.Fail($"Shift code '{req.Code}' already exists.");

        var shift = new Shift
        {
            Name = req.Name.Trim(),
            Code = req.Code.Trim().ToUpperInvariant(),
            StartTime = req.StartTime,
            EndTime = req.EndTime,
            GraceMinutes = req.GraceMinutes,
            IsNightShift = req.IsNightShift,
            WorkingHours = req.WorkingHours,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await uow.Repository<Shift>().AddAsync(shift);
        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Create, "Shifts", shift.Id.ToString(), null, shift);

        await cache.RemoveAsync("org:shifts:all");

        return await GetShiftByIdAsync(shift.Id);
    }

    public async Task<ApiResponse<ShiftDto>> UpdateShiftAsync(int id, UpdateShiftRequest req, int? userId)
    {
        var shift = await uow.Repository<Shift>().GetByIdAsync(id);
        if (shift == null)
            return ApiResponse<ShiftDto>.Fail("Shift not found.");

        shift.Name = req.Name.Trim();
        shift.Code = req.Code.Trim().ToUpperInvariant();
        shift.StartTime = req.StartTime;
        shift.EndTime = req.EndTime;
        shift.GraceMinutes = req.GraceMinutes;
        shift.IsNightShift = req.IsNightShift;
        shift.WorkingHours = req.WorkingHours;
        shift.IsActive = req.IsActive;
        shift.UpdatedAt = DateTime.UtcNow;
        shift.UpdatedBy = userId;

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Shifts", shift.Id.ToString(), null, shift);

        await cache.RemoveAsync("org:shifts:all");

        return await GetShiftByIdAsync(shift.Id);
    }

    public async Task<ApiResponse> DeleteShiftAsync(int id, int? userId)
    {
        var shift = await uow.Repository<Shift>().GetByIdAsync(id);
        if (shift == null)
            return ApiResponse.Fail("Shift not found.");

        shift.IsDeleted = true;
        shift.UpdatedAt = DateTime.UtcNow;
        shift.UpdatedBy = userId;

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Delete, "Shifts", shift.Id.ToString(), null, null);

        await cache.RemoveAsync("org:shifts:all");

        return ApiResponse.Ok("Shift deleted successfully.");
    }

    // ==========================================
    // HOLIDAYS
    // ==========================================
    public async Task<ApiResponse<List<HolidayDto>>> GetHolidaysAsync(int? year, int? branchId)
    {
        var cacheKey = $"org:holidays:{year ?? 0}:{branchId ?? 0}";
        return await cache.GetOrCreateAsync(cacheKey, async () =>
        {
            var q = uow.Repository<Holiday>().Query();

            if (year.HasValue)
                q = q.Where(h => h.HolidayDate.Year == year.Value);

            if (branchId.HasValue)
                q = q.Where(h => h.BranchId == null || h.BranchId == branchId.Value);

            var items = await q
                .OrderBy(h => h.HolidayDate)
                .Select(h => new HolidayDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    HolidayDate = h.HolidayDate,
                    Type = h.Type,
                    BranchId = h.BranchId,
                    BranchName = h.Branch != null ? h.Branch.Name : "All Branches",
                    IsRecurring = h.IsRecurring,
                    Description = h.Description,
                    CreatedAt = h.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<HolidayDto>>.Ok(items);
        }, TimeSpan.FromHours(1));
    }

    public async Task<ApiResponse<HolidayDto>> GetHolidayByIdAsync(int id)
    {
        var h = await uow.Repository<Holiday>().Query()
            .Include(x => x.Branch)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (h == null)
            return ApiResponse<HolidayDto>.Fail("Holiday not found.");

        return ApiResponse<HolidayDto>.Ok(new HolidayDto
        {
            Id = h.Id,
            Name = h.Name,
            HolidayDate = h.HolidayDate,
            Type = h.Type,
            BranchId = h.BranchId,
            BranchName = h.Branch != null ? h.Branch.Name : "All Branches",
            IsRecurring = h.IsRecurring,
            Description = h.Description,
            CreatedAt = h.CreatedAt
        });
    }

    public async Task<ApiResponse<HolidayDto>> CreateHolidayAsync(CreateHolidayRequest req, int? userId)
    {
        var holiday = new Holiday
        {
            Name = req.Name.Trim(),
            HolidayDate = req.HolidayDate,
            Type = req.Type,
            BranchId = req.BranchId,
            IsRecurring = req.IsRecurring,
            Description = req.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await uow.Repository<Holiday>().AddAsync(holiday);
        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Create, "Holidays", holiday.Id.ToString(), null, holiday);

        await InvalidateHolidayCacheAsync(holiday.HolidayDate.Year, holiday.BranchId);

        return await GetHolidayByIdAsync(holiday.Id);
    }

    public async Task<ApiResponse<HolidayDto>> UpdateHolidayAsync(int id, UpdateHolidayRequest req, int? userId)
    {
        var holiday = await uow.Repository<Holiday>().GetByIdAsync(id);
        if (holiday == null)
            return ApiResponse<HolidayDto>.Fail("Holiday not found.");

        var oldYear = holiday.HolidayDate.Year;
        var oldBranch = holiday.BranchId;

        holiday.Name = req.Name.Trim();
        holiday.HolidayDate = req.HolidayDate;
        holiday.Type = req.Type;
        holiday.BranchId = req.BranchId;
        holiday.IsRecurring = req.IsRecurring;
        holiday.Description = req.Description;
        holiday.UpdatedAt = DateTime.UtcNow;
        holiday.UpdatedBy = userId;

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Holidays", holiday.Id.ToString(), null, holiday);

        await InvalidateHolidayCacheAsync(oldYear, oldBranch);
        if (req.HolidayDate.Year != oldYear || req.BranchId != oldBranch)
            await InvalidateHolidayCacheAsync(req.HolidayDate.Year, req.BranchId);

        return await GetHolidayByIdAsync(holiday.Id);
    }

    public async Task<ApiResponse> DeleteHolidayAsync(int id, int? userId)
    {
        var holiday = await uow.Repository<Holiday>().GetByIdAsync(id);
        if (holiday == null)
            return ApiResponse.Fail("Holiday not found.");

        holiday.IsDeleted = true;
        holiday.UpdatedAt = DateTime.UtcNow;
        holiday.UpdatedBy = userId;

        await uow.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Delete, "Holidays", holiday.Id.ToString(), null, null);

        await InvalidateHolidayCacheAsync(holiday.HolidayDate.Year, holiday.BranchId);

        return ApiResponse.Ok("Holiday deleted successfully.");
    }

    private async Task InvalidateHolidayCacheAsync(int year, int? branchId)
    {
        await cache.RemoveAsync($"org:holidays:{year}:{branchId ?? 0}");
        await cache.RemoveAsync($"org:holidays:{year}:0");
        await cache.RemoveAsync($"org:holidays:0:{branchId ?? 0}");
        await cache.RemoveAsync("org:holidays:0:0");
    }
}
