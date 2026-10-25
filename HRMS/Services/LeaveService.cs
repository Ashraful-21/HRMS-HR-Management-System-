using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.Data;
using HRMS.Dtos.Common;
using HRMS.DTOs;
using HRMS.IServices;
using HRMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRMS.Services;

public class LeaveService(
    HrmsDbContext db,
    IAuditService auditService,
    ILogger<LeaveService> logger) : ILeaveService
{
    private async Task<Employee?> ResolveEmployeeAsync(int? requestedEmpId, int currentUserId)
    {
        if (requestedEmpId.HasValue && requestedEmpId.Value > 0)
        {
            return await db.Employees
                .Include(e => e.Department)
                .Include(e => e.Branch)
                .FirstOrDefaultAsync(e => e.Id == requestedEmpId.Value);
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
        if (user?.EmployeeId != null)
        {
            return await db.Employees
                .Include(e => e.Department)
                .Include(e => e.Branch)
                .FirstOrDefaultAsync(e => e.Id == user.EmployeeId.Value);
        }

        return await db.Employees
            .Include(e => e.Department)
            .Include(e => e.Branch)
            .FirstOrDefaultAsync(e => e.Status == "Active");
    }

    // ==========================================
    // 1. LEAVE TYPES
    // ==========================================
    public async Task<ApiResponse<List<LeaveTypeDto>>> GetLeaveTypesAsync()
    {
        var types = await db.LeaveTypes
            .OrderBy(t => t.Name)
            .Select(t => new LeaveTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                DaysAllowed = t.DaysAllowed,
                IsCarryForwardAllowed = t.IsCarryForwardAllowed,
                MaxCarryForwardDays = t.MaxCarryForwardDays,
                IsPaidLeave = t.IsPaidLeave,
                IsHalfDayAllowed = t.IsHalfDayAllowed,
                IsAttachmentRequired = t.IsAttachmentRequired,
                AttachmentRequiredAfterDays = t.AttachmentRequiredAfterDays,
                GenderRestriction = t.GenderRestriction,
                IsActive = t.IsActive
            })
            .ToListAsync();

        return ApiResponse<List<LeaveTypeDto>>.Ok(types);
    }

    public async Task<ApiResponse<LeaveTypeDto>> CreateLeaveTypeAsync(CreateLeaveTypeRequest req, int currentUserId)
    {
        var code = req.Code.Trim().ToUpper();
        if (await db.LeaveTypes.AnyAsync(t => t.Code == code))
        {
            return ApiResponse<LeaveTypeDto>.Fail($"Leave type with code '{code}' already exists.");
        }

        var lt = new LeaveType
        {
            Name = req.Name.Trim(),
            Code = code,
            DaysAllowed = req.DaysAllowed,
            IsCarryForwardAllowed = req.IsCarryForwardAllowed,
            MaxCarryForwardDays = req.IsCarryForwardAllowed ? req.MaxCarryForwardDays : null,
            IsPaidLeave = req.IsPaidLeave,
            IsHalfDayAllowed = req.IsHalfDayAllowed,
            IsAttachmentRequired = req.IsAttachmentRequired,
            AttachmentRequiredAfterDays = req.IsAttachmentRequired ? req.AttachmentRequiredAfterDays : null,
            GenderRestriction = string.IsNullOrWhiteSpace(req.GenderRestriction) ? null : req.GenderRestriction.Trim(),
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        db.LeaveTypes.Add(lt);
        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "CREATE_LEAVE_TYPE",
            "LeaveType",
            lt.Id.ToString(),
            null,
            new { lt.Id, lt.Name, lt.Code, lt.DaysAllowed });

        return ApiResponse<LeaveTypeDto>.Ok(new LeaveTypeDto
        {
            Id = lt.Id,
            Name = lt.Name,
            Code = lt.Code,
            DaysAllowed = lt.DaysAllowed,
            IsCarryForwardAllowed = lt.IsCarryForwardAllowed,
            MaxCarryForwardDays = lt.MaxCarryForwardDays,
            IsPaidLeave = lt.IsPaidLeave,
            IsHalfDayAllowed = lt.IsHalfDayAllowed,
            IsAttachmentRequired = lt.IsAttachmentRequired,
            AttachmentRequiredAfterDays = lt.AttachmentRequiredAfterDays,
            GenderRestriction = lt.GenderRestriction,
            IsActive = lt.IsActive
        }, "Leave type created successfully.");
    }

    public async Task<ApiResponse<LeaveTypeDto>> UpdateLeaveTypeAsync(int id, UpdateLeaveTypeRequest req, int currentUserId)
    {
        var lt = await db.LeaveTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (lt == null)
            return ApiResponse<LeaveTypeDto>.Fail("Leave type not found.");

        lt.Name = req.Name.Trim();
        lt.DaysAllowed = req.DaysAllowed;
        lt.IsCarryForwardAllowed = req.IsCarryForwardAllowed;
        lt.MaxCarryForwardDays = req.IsCarryForwardAllowed ? req.MaxCarryForwardDays : null;
        lt.IsPaidLeave = req.IsPaidLeave;
        lt.IsHalfDayAllowed = req.IsHalfDayAllowed;
        lt.IsAttachmentRequired = req.IsAttachmentRequired;
        lt.AttachmentRequiredAfterDays = req.IsAttachmentRequired ? req.AttachmentRequiredAfterDays : null;
        lt.GenderRestriction = string.IsNullOrWhiteSpace(req.GenderRestriction) ? null : req.GenderRestriction.Trim();
        lt.IsActive = req.IsActive;
        lt.UpdatedAt = DateTime.UtcNow;
        lt.UpdatedBy = currentUserId;

        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "UPDATE_LEAVE_TYPE",
            "LeaveType",
            lt.Id.ToString(),
            null,
            new { lt.Id, lt.Name, lt.DaysAllowed });

        return ApiResponse<LeaveTypeDto>.Ok(new LeaveTypeDto
        {
            Id = lt.Id,
            Name = lt.Name,
            Code = lt.Code,
            DaysAllowed = lt.DaysAllowed,
            IsCarryForwardAllowed = lt.IsCarryForwardAllowed,
            MaxCarryForwardDays = lt.MaxCarryForwardDays,
            IsPaidLeave = lt.IsPaidLeave,
            IsHalfDayAllowed = lt.IsHalfDayAllowed,
            IsAttachmentRequired = lt.IsAttachmentRequired,
            AttachmentRequiredAfterDays = lt.AttachmentRequiredAfterDays,
            GenderRestriction = lt.GenderRestriction,
            IsActive = lt.IsActive
        }, "Leave type updated successfully.");
    }

    public async Task<ApiResponse<string>> DeleteLeaveTypeAsync(int id, int currentUserId)
    {
        var lt = await db.LeaveTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (lt == null)
            return ApiResponse<string>.Fail("Leave type not found.");

        lt.IsDeleted = true;
        lt.IsActive = false;
        lt.UpdatedAt = DateTime.UtcNow;
        lt.UpdatedBy = currentUserId;

        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "DELETE_LEAVE_TYPE",
            "LeaveType",
            lt.Id.ToString(),
            null,
            new { lt.Id, lt.Name, lt.Code });

        return ApiResponse<string>.Ok("Leave type soft-deleted successfully.");
    }

    // ==========================================
    // 2. LEAVE BALANCES
    // ==========================================
    public async Task<ApiResponse<List<LeaveBalanceDto>>> GetEmployeeLeaveBalancesAsync(int employeeId, int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;

        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee == null)
            return ApiResponse<List<LeaveBalanceDto>>.Fail("Employee not found.");

        var balances = await db.LeaveBalances
            .Include(b => b.LeaveType)
            .Include(b => b.Employee)
            .Where(b => b.EmployeeId == employeeId && b.Year == targetYear)
            .ToListAsync();

        // Auto-initialize if employee has no balances recorded for this year
        if (balances.Count == 0)
        {
            var activeTypes = await db.LeaveTypes.Where(t => t.IsActive).ToListAsync();
            foreach (var t in activeTypes)
            {
                // Check gender restriction before allocating gender-specific leave types
                if (!string.IsNullOrEmpty(t.GenderRestriction) &&
                    !string.Equals(employee.Gender, t.GenderRestriction, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var newBal = new LeaveBalance
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = t.Id,
                    Year = targetYear,
                    TotalDays = t.DaysAllowed,
                    CarryForwardDays = 0,
                    UsedDays = 0,
                    CreatedAt = DateTime.UtcNow
                };
                db.LeaveBalances.Add(newBal);
            }
            await db.SaveChangesAsync();

            balances = await db.LeaveBalances
                .Include(b => b.LeaveType)
                .Include(b => b.Employee)
                .Where(b => b.EmployeeId == employeeId && b.Year == targetYear)
                .ToListAsync();
        }

        var dtos = balances.Select(b => new LeaveBalanceDto
        {
            Id = b.Id,
            EmployeeId = b.EmployeeId,
            EmployeeName = b.Employee.FullName,
            EmployeeCode = b.Employee.EmployeeCode,
            LeaveTypeId = b.LeaveTypeId,
            LeaveTypeName = b.LeaveType.Name,
            LeaveTypeCode = b.LeaveType.Code,
            Year = b.Year,
            TotalDays = b.TotalDays,
            CarryForwardDays = b.CarryForwardDays,
            UsedDays = b.UsedDays,
            Balance = b.Balance ?? (b.TotalDays + b.CarryForwardDays - b.UsedDays)
        }).ToList();

        return ApiResponse<List<LeaveBalanceDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<string>> AllocateLeaveBalanceAsync(AllocateLeaveBalanceRequest req, int currentUserId)
    {
        var lt = await db.LeaveTypes.FirstOrDefaultAsync(t => t.Id == req.LeaveTypeId);
        if (lt == null)
            return ApiResponse<string>.Fail("Leave type not found.");

        var employees = req.EmployeeId.HasValue
            ? await db.Employees.Where(e => e.Id == req.EmployeeId.Value).ToListAsync()
            : await db.Employees.Where(e => e.Status == "Active").ToListAsync();

        int count = 0;
        foreach (var emp in employees)
        {
            var bal = await db.LeaveBalances
                .FirstOrDefaultAsync(b => b.EmployeeId == emp.Id && b.LeaveTypeId == req.LeaveTypeId && b.Year == req.Year);

            if (bal == null)
            {
                bal = new LeaveBalance
                {
                    EmployeeId = emp.Id,
                    LeaveTypeId = req.LeaveTypeId,
                    Year = req.Year,
                    TotalDays = req.Days,
                    CarryForwardDays = req.CarryForwardDays,
                    UsedDays = 0,
                    CreatedAt = DateTime.UtcNow
                };
                db.LeaveBalances.Add(bal);
            }
            else
            {
                bal.TotalDays = req.Days;
                bal.CarryForwardDays = req.CarryForwardDays;
                bal.UpdatedAt = DateTime.UtcNow;
            }
            count++;
        }

        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "ALLOCATE_LEAVE_BALANCE",
            "LeaveBalance",
            req.LeaveTypeId.ToString(),
            null,
            new { req.LeaveTypeId, req.Year, req.Days, req.CarryForwardDays, ProcessedCount = count });

        return ApiResponse<string>.Ok($"Leave balance allocated for {count} employee(s).");
    }

    public async Task<ApiResponse<string>> ProcessYearEndCarryForwardAsync(YearEndCarryForwardRequest req, int currentUserId)
    {
        logger.LogInformation("Processing year-end carry forward from {FromYear} to {ToYear}", req.FromYear, req.ToYear);

        var carryForwardTypes = await db.LeaveTypes
            .Where(t => t.IsActive && t.IsCarryForwardAllowed)
            .ToListAsync();

        int processed = 0;
        foreach (var lt in carryForwardTypes)
        {
            var fromBalances = await db.LeaveBalances
                .Where(b => b.LeaveTypeId == lt.Id && b.Year == req.FromYear)
                .ToListAsync();

            foreach (var fb in fromBalances)
            {
                var remaining = Math.Max(0, (fb.TotalDays + fb.CarryForwardDays - fb.UsedDays));
                if (remaining <= 0) continue;

                var carryOver = lt.MaxCarryForwardDays.HasValue
                    ? Math.Min(remaining, lt.MaxCarryForwardDays.Value)
                    : remaining;

                var toBal = await db.LeaveBalances
                    .FirstOrDefaultAsync(b => b.EmployeeId == fb.EmployeeId && b.LeaveTypeId == lt.Id && b.Year == req.ToYear);

                if (toBal == null)
                {
                    toBal = new LeaveBalance
                    {
                        EmployeeId = fb.EmployeeId,
                        LeaveTypeId = lt.Id,
                        Year = req.ToYear,
                        TotalDays = lt.DaysAllowed,
                        CarryForwardDays = carryOver,
                        UsedDays = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    db.LeaveBalances.Add(toBal);
                }
                else
                {
                    toBal.CarryForwardDays = carryOver;
                    toBal.UpdatedAt = DateTime.UtcNow;
                }

                processed++;
            }
        }

        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "YEAR_END_CARRY_FORWARD",
            "LeaveBalance",
            req.ToYear.ToString(),
            null,
            new { req.FromYear, req.ToYear, ProcessedRecords = processed });

        return ApiResponse<string>.Ok($"Year-end carry forward completed. Processed {processed} record(s).");
    }

    // ==========================================
    // 3. LEAVE APPLICATIONS
    // ==========================================
    public async Task<ApiResponse<LeaveApplicationDto>> ApplyLeaveAsync(ApplyLeaveRequest req, int currentUserId)
    {
        var employee = await ResolveEmployeeAsync(req.EmployeeId, currentUserId);
        if (employee == null)
            return ApiResponse<LeaveApplicationDto>.Fail("Employee record not found.");

        var leaveType = await db.LeaveTypes.FirstOrDefaultAsync(t => t.Id == req.LeaveTypeId && t.IsActive);
        if (leaveType == null)
            return ApiResponse<LeaveApplicationDto>.Fail("Invalid or inactive leave type selected.");

        if (req.EndDate < req.StartDate)
            return ApiResponse<LeaveApplicationDto>.Fail("End date must be on or after start date.");

        // Half-day validation
        decimal totalDays;
        if (req.IsHalfDay)
        {
            if (!leaveType.IsHalfDayAllowed)
                return ApiResponse<LeaveApplicationDto>.Fail($"Half-day leave is not permitted for {leaveType.Name}.");

            if (req.StartDate != req.EndDate)
                return ApiResponse<LeaveApplicationDto>.Fail("Half-day leave must start and end on the same date.");

            totalDays = 0.5m;
        }
        else
        {
            decimal workingDays = 0;
            for (var d = req.StartDate; d <= req.EndDate; d = d.AddDays(1))
            {
                // Exclude weekends (Friday & Saturday)
                if (d.DayOfWeek == DayOfWeek.Friday || d.DayOfWeek == DayOfWeek.Saturday)
                    continue;

                workingDays += 1m;
            }

            totalDays = Math.Max(1m, workingDays);
        }

        // Gender restriction check
        if (!string.IsNullOrEmpty(leaveType.GenderRestriction) &&
            !string.Equals(employee.Gender, leaveType.GenderRestriction, StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<LeaveApplicationDto>.Fail($"'{leaveType.Name}' is restricted to {leaveType.GenderRestriction} employees.");
        }

        // Supporting attachment requirement check
        if (leaveType.IsAttachmentRequired && leaveType.AttachmentRequiredAfterDays.HasValue &&
            totalDays >= leaveType.AttachmentRequiredAfterDays.Value && string.IsNullOrWhiteSpace(req.AttachmentUrl))
        {
            return ApiResponse<LeaveApplicationDto>.Fail($"A medical or supporting document attachment is required for leaves of {leaveType.AttachmentRequiredAfterDays.Value} days or longer.");
        }

        // Overlapping active leave check
        var hasOverlap = await db.LeaveApplications
            .AnyAsync(l => l.EmployeeId == employee.Id &&
                           l.Status != "Rejected" &&
                           l.Status != "Cancelled" &&
                           l.StartDate <= req.EndDate &&
                           l.EndDate >= req.StartDate);

        if (hasOverlap)
        {
            return ApiResponse<LeaveApplicationDto>.Fail("An existing active or approved leave application overlaps with the selected date range.");
        }

        // Leave balance check for paid leave
        var currentYear = req.StartDate.Year;
        var balance = await db.LeaveBalances
            .FirstOrDefaultAsync(b => b.EmployeeId == employee.Id && b.LeaveTypeId == leaveType.Id && b.Year == currentYear);

        if (balance == null)
        {
            // Auto-initialize balance
            balance = new LeaveBalance
            {
                EmployeeId = employee.Id,
                LeaveTypeId = leaveType.Id,
                Year = currentYear,
                TotalDays = leaveType.DaysAllowed,
                CarryForwardDays = 0,
                UsedDays = 0,
                CreatedAt = DateTime.UtcNow
            };
            db.LeaveBalances.Add(balance);
            await db.SaveChangesAsync();
        }

        var available = (balance.TotalDays + balance.CarryForwardDays - balance.UsedDays);
        if (leaveType.IsPaidLeave && available < totalDays)
        {
            return ApiResponse<LeaveApplicationDto>.Fail($"Insufficient leave balance. Available: {available:0.#} days, Requested: {totalDays:0.#} days.");
        }

        var app = new LeaveApplication
        {
            EmployeeId = employee.Id,
            LeaveTypeId = leaveType.Id,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            IsHalfDay = req.IsHalfDay,
            HalfDayType = req.IsHalfDay ? req.HalfDayType : null,
            TotalDays = totalDays,
            Reason = req.Reason.Trim(),
            Status = "Pending",
            AttachmentUrl = req.AttachmentUrl,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        db.LeaveApplications.Add(app);
        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "APPLY_LEAVE",
            "LeaveApplication",
            app.Id.ToString(),
            null,
            new { app.Id, app.EmployeeId, app.LeaveTypeId, app.StartDate, app.EndDate, app.TotalDays });

        var dto = MapToDto(app, employee, leaveType, null, null);
        return ApiResponse<LeaveApplicationDto>.Ok(dto, "Leave application submitted successfully.");
    }

    public async Task<ApiResponse<PagedResult<LeaveApplicationDto>>> GetLeaveApplicationsAsync(LeaveApplicationFilterDto filter, int currentUserId)
    {
        var q = db.LeaveApplications
            .Include(l => l.Employee)
                .ThenInclude(e => e.Department)
            .Include(l => l.LeaveType)
            .Include(l => l.ApprovedByNavigation)
            .Include(l => l.RejectedByNavigation)
            .AsQueryable();

        if (filter.EmployeeId.HasValue)
            q = q.Where(l => l.EmployeeId == filter.EmployeeId.Value);

        if (filter.LeaveTypeId.HasValue)
            q = q.Where(l => l.LeaveTypeId == filter.LeaveTypeId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            q = q.Where(l => l.Status == filter.Status);

        if (filter.StartDate.HasValue)
            q = q.Where(l => l.EndDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            q = q.Where(l => l.StartDate <= filter.EndDate.Value);

        var total = await q.CountAsync();
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await q.OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(l => MapToDto(l, l.Employee, l.LeaveType, l.ApprovedByNavigation, l.RejectedByNavigation)).ToList();

        var result = new PagedResult<LeaveApplicationDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<LeaveApplicationDto>>.Ok(result);
    }

    public async Task<ApiResponse<LeaveApplicationDto>> GetLeaveApplicationByIdAsync(int id)
    {
        var app = await db.LeaveApplications
            .Include(l => l.Employee)
                .ThenInclude(e => e.Department)
            .Include(l => l.LeaveType)
            .Include(l => l.ApprovedByNavigation)
            .Include(l => l.RejectedByNavigation)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (app == null)
            return ApiResponse<LeaveApplicationDto>.Fail("Leave application not found.");

        return ApiResponse<LeaveApplicationDto>.Ok(MapToDto(app, app.Employee, app.LeaveType, app.ApprovedByNavigation, app.RejectedByNavigation));
    }

    public async Task<ApiResponse<LeaveApplicationDto>> ReviewLeaveApplicationAsync(int id, ReviewLeaveApplicationRequest req, int reviewerUserId)
    {
        var app = await db.LeaveApplications
            .Include(l => l.Employee)
                .ThenInclude(e => e.Department)
            .Include(l => l.LeaveType)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (app == null)
            return ApiResponse<LeaveApplicationDto>.Fail("Leave application not found.");

        if (app.Status != "Pending")
            return ApiResponse<LeaveApplicationDto>.Fail($"Leave application has already been {app.Status}.");

        var reviewerEmp = await ResolveEmployeeAsync(null, reviewerUserId);

        app.Status = req.Status;
        app.ApprovalRemarks = req.Remarks;
        app.UpdatedAt = DateTime.UtcNow;
        app.UpdatedBy = reviewerUserId;

        if (req.Status == "Approved")
        {
            app.ApprovedBy = reviewerEmp?.Id;
            app.ApprovedAt = DateTime.UtcNow;

            // 1. Deduct leave balance
            var balance = await db.LeaveBalances
                .FirstOrDefaultAsync(b => b.EmployeeId == app.EmployeeId && b.LeaveTypeId == app.LeaveTypeId && b.Year == app.StartDate.Year);

            if (balance != null)
            {
                balance.UsedDays += app.TotalDays;
                balance.UpdatedAt = DateTime.UtcNow;
            }

            // 2. Mark attendance logs as 'OnLeave' for each day in range
            for (var d = app.StartDate; d <= app.EndDate; d = d.AddDays(1))
            {
                var log = await db.AttendanceLogs
                    .FirstOrDefaultAsync(a => a.EmployeeId == app.EmployeeId && a.LogDate == d);

                if (log == null)
                {
                    log = new AttendanceLog
                    {
                        EmployeeId = app.EmployeeId,
                        LogDate = d,
                        ShiftId = app.Employee.ShiftId,
                        Status = "OnLeave",
                        Remarks = $"Approved Leave ({app.LeaveType.Name})",
                        CreatedAt = DateTime.UtcNow
                    };
                    db.AttendanceLogs.Add(log);
                }
                else
                {
                    log.Status = "OnLeave";
                    log.Remarks = $"Approved Leave ({app.LeaveType.Name})";
                    log.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
        else if (req.Status == "Rejected")
        {
            app.RejectedBy = reviewerEmp?.Id;
            app.RejectedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "REVIEW_LEAVE_APPLICATION",
            "LeaveApplication",
            app.Id.ToString(),
            null,
            new { app.Id, app.EmployeeId, Status = req.Status, req.Remarks, reviewerUserId });

        var dto = MapToDto(app, app.Employee, app.LeaveType, reviewerEmp, req.Status == "Rejected" ? reviewerEmp : null);
        return ApiResponse<LeaveApplicationDto>.Ok(dto, $"Leave application {req.Status.ToLower()} successfully.");
    }

    public async Task<ApiResponse<string>> CancelLeaveApplicationAsync(int id, int currentUserId)
    {
        var app = await db.LeaveApplications
            .Include(l => l.LeaveType)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (app == null)
            return ApiResponse<string>.Fail("Leave application not found.");

        if (app.Status == "Cancelled" || app.Status == "Rejected")
            return ApiResponse<string>.Fail($"Application is already {app.Status}.");

        // If it was already approved, refund the used days and reset attendance logs
        if (app.Status == "Approved")
        {
            var balance = await db.LeaveBalances
                .FirstOrDefaultAsync(b => b.EmployeeId == app.EmployeeId && b.LeaveTypeId == app.LeaveTypeId && b.Year == app.StartDate.Year);

            if (balance != null)
            {
                balance.UsedDays = Math.Max(0, balance.UsedDays - app.TotalDays);
                balance.UpdatedAt = DateTime.UtcNow;
            }

            // Clear OnLeave status from logs
            for (var d = app.StartDate; d <= app.EndDate; d = d.AddDays(1))
            {
                var log = await db.AttendanceLogs
                    .FirstOrDefaultAsync(a => a.EmployeeId == app.EmployeeId && a.LogDate == d && a.Status == "OnLeave");

                if (log != null)
                {
                    log.Status = "Absent";
                    log.Remarks = "Leave Cancelled";
                    log.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        app.Status = "Cancelled";
        app.UpdatedAt = DateTime.UtcNow;
        app.UpdatedBy = currentUserId;

        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "CANCEL_LEAVE_APPLICATION",
            "LeaveApplication",
            app.Id.ToString(),
            null,
            new { app.Id, app.EmployeeId });

        return ApiResponse<string>.Ok("Leave application cancelled successfully.");
    }

    private static LeaveApplicationDto MapToDto(
        LeaveApplication app,
        Employee emp,
        LeaveType lt,
        Employee? approver,
        Employee? rejecter)
    {
        return new LeaveApplicationDto
        {
            Id = app.Id,
            EmployeeId = app.EmployeeId,
            EmployeeName = emp.FullName,
            EmployeeCode = emp.EmployeeCode,
            DepartmentName = emp.Department?.Name,
            LeaveTypeId = app.LeaveTypeId,
            LeaveTypeName = lt.Name,
            LeaveTypeCode = lt.Code,
            StartDate = app.StartDate,
            EndDate = app.EndDate,
            IsHalfDay = app.IsHalfDay,
            HalfDayType = app.HalfDayType,
            TotalDays = app.TotalDays,
            Reason = app.Reason,
            Status = app.Status,
            AttachmentUrl = app.AttachmentUrl,
            ApprovedBy = app.ApprovedBy,
            ApproverName = approver?.FullName ?? app.ApprovedByNavigation?.FullName,
            ApprovedAt = app.ApprovedAt,
            RejectedBy = app.RejectedBy,
            RejecterName = rejecter?.FullName ?? app.RejectedByNavigation?.FullName,
            RejectedAt = app.RejectedAt,
            ApprovalRemarks = app.ApprovalRemarks,
            CreatedAt = app.CreatedAt
        };
    }
}
