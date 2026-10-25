using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.Constants;
using HRMS.Data;
using HRMS.Dtos.Common;
using HRMS.DTOs;
using HRMS.IServices;
using HRMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRMS.Services;

public class AttendanceService(
    HrmsDbContext db,
    IAuditService auditService,
    ILogger<AttendanceService> logger) : IAttendanceService
{
    private async Task<Employee?> ResolveEmployeeAsync(int? requestedEmpId, int currentUserId)
    {
        if (requestedEmpId.HasValue && requestedEmpId.Value > 0)
        {
            return await db.Employees
                .Include(e => e.Shift)
                .Include(e => e.Branch)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == requestedEmpId.Value);
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
        if (user?.EmployeeId != null)
        {
            return await db.Employees
                .Include(e => e.Shift)
                .Include(e => e.Branch)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == user.EmployeeId.Value);
        }

        // Fallback: first active employee if superadmin has no direct Employee link
        return await db.Employees
            .Include(e => e.Shift)
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Status == "Active");
    }

    public async Task<ApiResponse<AttendanceLogResponseDto>> CheckInAsync(CheckInRequest req, string clientIp, int currentUserId)
    {
        var employee = await ResolveEmployeeAsync(req.EmployeeId, currentUserId);
        if (employee == null)
            return ApiResponse<AttendanceLogResponseDto>.Fail("No associated employee record found.");

        var checkInTime = req.CheckInTime ?? DateTime.UtcNow;
        var today = DateOnly.FromDateTime(checkInTime);

        // Check if already checked in
        var existingLog = await db.AttendanceLogs
            .Include(a => a.Shift)
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.LogDate == today);

        if (existingLog != null && existingLog.CheckInTime.HasValue)
        {
            return ApiResponse<AttendanceLogResponseDto>.Fail("Employee has already checked in for today.");
        }

        var shift = employee.Shift ?? await db.Shifts.FirstOrDefaultAsync(s => s.IsActive);
        var shiftStartTime = shift?.StartTime ?? new TimeOnly(9, 0);
        var graceMinutes = shift?.GraceMinutes ?? 15;

        var checkInTimeOfDay = TimeOnly.FromDateTime(checkInTime);
        var graceThreshold = shiftStartTime.AddMinutes(graceMinutes);
        bool isLate = checkInTimeOfDay > graceThreshold;
        int lateMinutes = 0;
        if (isLate)
        {
            lateMinutes = (int)Math.Max(0, (checkInTimeOfDay.ToTimeSpan() - shiftStartTime.ToTimeSpan()).TotalMinutes);
        }

        var log = existingLog;
        if (log == null)
        {
            log = new AttendanceLog
            {
                EmployeeId = employee.Id,
                LogDate = today,
                ShiftId = shift?.Id,
                CreatedAt = DateTime.UtcNow
            };
            db.AttendanceLogs.Add(log);
        }

        log.CheckInTime = checkInTime;
        log.CheckInIp = clientIp;
        log.CheckInLat = req.Latitude;
        log.CheckInLng = req.Longitude;
        log.IsLate = isLate;
        log.LateMinutes = lateMinutes;
        log.Status = isLate ? "Late" : "Present";
        if (!string.IsNullOrWhiteSpace(req.Remarks))
            log.Remarks = req.Remarks;
        log.CreatedBy = currentUserId;

        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "CHECK_IN",
            "AttendanceLog",
            log.Id.ToString(),
            null,
            new { log.Id, log.EmployeeId, log.LogDate, log.CheckInTime, log.IsLate, log.LateMinutes, clientIp });

        return ApiResponse<AttendanceLogResponseDto>.Ok(MapToDto(log, employee, shift));
    }

    public async Task<ApiResponse<AttendanceLogResponseDto>> CheckOutAsync(CheckOutRequest req, string clientIp, int currentUserId)
    {
        var employee = await ResolveEmployeeAsync(req.EmployeeId, currentUserId);
        if (employee == null)
            return ApiResponse<AttendanceLogResponseDto>.Fail("No associated employee record found.");

        var checkOutTime = req.CheckOutTime ?? DateTime.UtcNow;
        var today = DateOnly.FromDateTime(checkOutTime);

        var log = await db.AttendanceLogs
            .Include(a => a.Shift)
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .Include(a => a.Employee)
                .ThenInclude(e => e.Branch)
            .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.LogDate == today);

        if (log == null || !log.CheckInTime.HasValue)
        {
            return ApiResponse<AttendanceLogResponseDto>.Fail("No check-in record found for today.");
        }

        if (log.CheckOutTime.HasValue)
        {
            return ApiResponse<AttendanceLogResponseDto>.Fail("Employee has already checked out for today.");
        }

        var shift = log.Shift ?? employee.Shift ?? await db.Shifts.FirstOrDefaultAsync(s => s.IsActive);
        var shiftEndTime = shift?.EndTime ?? new TimeOnly(18, 0);
        var standardWorkHours = shift?.WorkingHours ?? 8m;
        var standardWorkMinutes = (int)(standardWorkHours * 60);

        log.CheckOutTime = checkOutTime;
        log.CheckOutIp = clientIp;

        var duration = checkOutTime - log.CheckInTime.Value;
        log.WorkingMinutes = (int)Math.Max(0, duration.TotalMinutes);

        var checkOutTimeOfDay = TimeOnly.FromDateTime(checkOutTime);
        if (checkOutTimeOfDay < shiftEndTime)
        {
            log.IsEarlyLeave = true;
            log.EarlyLeaveMinutes = (int)Math.Max(0, (shiftEndTime.ToTimeSpan() - checkOutTimeOfDay.ToTimeSpan()).TotalMinutes);
        }

        if (log.WorkingMinutes > standardWorkMinutes)
        {
            log.OvertimeMinutes = log.WorkingMinutes - standardWorkMinutes;
        }

        if (log.WorkingMinutes < (standardWorkMinutes / 2))
        {
            log.Status = "HalfDay";
        }
        else if (!log.IsLate)
        {
            log.Status = "Present";
        }

        if (!string.IsNullOrWhiteSpace(req.Remarks))
        {
            log.Remarks = string.IsNullOrEmpty(log.Remarks) ? req.Remarks : $"{log.Remarks} | {req.Remarks}";
        }

        log.UpdatedAt = DateTime.UtcNow;
        log.UpdatedBy = currentUserId;

        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "CHECK_OUT",
            "AttendanceLog",
            log.Id.ToString(),
            null,
            new { log.Id, log.EmployeeId, log.LogDate, log.CheckOutTime, log.WorkingMinutes, log.IsEarlyLeave, log.EarlyLeaveMinutes, log.OvertimeMinutes, clientIp });

        return ApiResponse<AttendanceLogResponseDto>.Ok(MapToDto(log, employee, shift));
    }

    public async Task<ApiResponse<TodayAttendanceStatusDto>> GetTodayStatusAsync(int currentUserId)
    {
        var employee = await ResolveEmployeeAsync(null, currentUserId);
        if (employee == null)
            return ApiResponse<TodayAttendanceStatusDto>.Fail("No associated employee record found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var log = await db.AttendanceLogs
            .Include(a => a.Shift)
            .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.LogDate == today);

        var shift = log?.Shift ?? employee.Shift ?? await db.Shifts.FirstOrDefaultAsync(s => s.IsActive);

        var result = new TodayAttendanceStatusDto
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.FullName,
            LogDate = today,
            HasCheckedIn = log?.CheckInTime.HasValue == true,
            HasCheckedOut = log?.CheckOutTime.HasValue == true,
            CheckInTime = log?.CheckInTime,
            CheckOutTime = log?.CheckOutTime,
            WorkingMinutes = log?.WorkingMinutes ?? 0,
            Status = log?.Status ?? "Absent",
            IsLate = log?.IsLate ?? false,
            LateMinutes = log?.LateMinutes ?? 0,
            ShiftName = shift?.Name,
            ShiftStartTime = shift?.StartTime,
            ShiftEndTime = shift?.EndTime,
            ShiftGraceMinutes = shift?.GraceMinutes ?? 15
        };

        return ApiResponse<TodayAttendanceStatusDto>.Ok(result);
    }

    public async Task<ApiResponse<AttendanceLogResponseDto>> ManualAttendanceAsync(ManualAttendanceRequest req, int currentUserId)
    {
        var employee = await db.Employees
            .Include(e => e.Shift)
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == req.EmployeeId);

        if (employee == null)
            return ApiResponse<AttendanceLogResponseDto>.Fail("Employee not found.");

        var shift = req.ShiftId.HasValue
            ? await db.Shifts.FirstOrDefaultAsync(s => s.Id == req.ShiftId.Value)
            : employee.Shift ?? await db.Shifts.FirstOrDefaultAsync(s => s.IsActive);

        var log = await db.AttendanceLogs.FirstOrDefaultAsync(a => a.EmployeeId == req.EmployeeId && a.LogDate == req.LogDate);
        if (log == null)
        {
            log = new AttendanceLog
            {
                EmployeeId = req.EmployeeId,
                LogDate = req.LogDate,
                CreatedAt = DateTime.UtcNow
            };
            db.AttendanceLogs.Add(log);
        }

        log.ShiftId = shift?.Id;
        log.CheckInTime = req.CheckInTime;
        log.CheckOutTime = req.CheckOutTime;
        log.Status = req.Status;
        log.IsManualEntry = true;
        log.Remarks = req.Remarks;

        if (req.CheckInTime.HasValue && shift != null)
        {
            var checkInTod = TimeOnly.FromDateTime(req.CheckInTime.Value);
            var graceThreshold = shift.StartTime.AddMinutes(shift.GraceMinutes);
            log.IsLate = checkInTod > graceThreshold;
            log.LateMinutes = log.IsLate ? (int)Math.Max(0, (checkInTod.ToTimeSpan() - shift.StartTime.ToTimeSpan()).TotalMinutes) : 0;
        }

        if (req.CheckInTime.HasValue && req.CheckOutTime.HasValue)
        {
            var duration = req.CheckOutTime.Value - req.CheckInTime.Value;
            log.WorkingMinutes = (int)Math.Max(0, duration.TotalMinutes);

            if (shift != null)
            {
                var checkOutTod = TimeOnly.FromDateTime(req.CheckOutTime.Value);
                if (checkOutTod < shift.EndTime)
                {
                    log.IsEarlyLeave = true;
                    log.EarlyLeaveMinutes = (int)Math.Max(0, (shift.EndTime.ToTimeSpan() - checkOutTod.ToTimeSpan()).TotalMinutes);
                }

                var standardMin = (int)(shift.WorkingHours * 60);
                if (log.WorkingMinutes > standardMin)
                    log.OvertimeMinutes = log.WorkingMinutes - standardMin;
            }
        }

        log.UpdatedAt = DateTime.UtcNow;
        log.UpdatedBy = currentUserId;

        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "MANUAL_ENTRY",
            "AttendanceLog",
            log.Id.ToString(),
            null,
            new { log.Id, req.EmployeeId, req.LogDate, req.Status, req.CheckInTime, req.CheckOutTime });

        return ApiResponse<AttendanceLogResponseDto>.Ok(MapToDto(log, employee, shift));
    }

    public async Task<ApiResponse<PagedResult<AttendanceLogResponseDto>>> GetAttendanceLogsAsync(AttendanceFilterDto filter)
    {
        var q = db.AttendanceLogs
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .Include(a => a.Employee)
                .ThenInclude(e => e.Branch)
            .Include(a => a.Shift)
            .AsQueryable();

        if (filter.EmployeeId.HasValue)
            q = q.Where(a => a.EmployeeId == filter.EmployeeId.Value);

        if (filter.DepartmentId.HasValue)
            q = q.Where(a => a.Employee.DepartmentId == filter.DepartmentId.Value);

        if (filter.BranchId.HasValue)
            q = q.Where(a => a.Employee.BranchId == filter.BranchId.Value);

        if (filter.StartDate.HasValue)
            q = q.Where(a => a.LogDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            q = q.Where(a => a.LogDate <= filter.EndDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            q = q.Where(a => a.Status == filter.Status);

        var total = await q.CountAsync();
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await q.OrderByDescending(a => a.LogDate)
            .ThenByDescending(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(a => MapToDto(a, a.Employee, a.Shift)).ToList();

        var result = new PagedResult<AttendanceLogResponseDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<AttendanceLogResponseDto>>.Ok(result);
    }

    public async Task<ApiResponse<AttendanceMonthlySummaryDto>> GetMonthlySummaryAsync(int employeeId, int year, int month)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee == null)
            return ApiResponse<AttendanceMonthlySummaryDto>.Fail("Employee not found.");

        var startDate = new DateOnly(year, month, 1);
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var endDate = new DateOnly(year, month, daysInMonth);

        var logs = await db.AttendanceLogs
            .Where(a => a.EmployeeId == employeeId && a.LogDate >= startDate && a.LogDate <= endDate)
            .ToListAsync();

        var summary = new AttendanceMonthlySummaryDto
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.FullName,
            Year = year,
            Month = month,
            TotalDaysInMonth = daysInMonth,
            PresentDays = logs.Count(a => a.Status == "Present"),
            LateDays = logs.Count(a => a.IsLate || a.Status == "Late"),
            EarlyLeaveDays = logs.Count(a => a.IsEarlyLeave),
            HalfDays = logs.Count(a => a.Status == "HalfDay"),
            AbsentDays = logs.Count(a => a.Status == "Absent"),
            HolidayDays = logs.Count(a => a.Status == "Holiday"),
            WeekendDays = logs.Count(a => a.Status == "Weekend"),
            OnLeaveDays = logs.Count(a => a.Status == "OnLeave"),
            TotalWorkingMinutes = logs.Sum(a => a.WorkingMinutes),
            TotalOvertimeMinutes = logs.Sum(a => a.OvertimeMinutes)
        };

        return ApiResponse<AttendanceMonthlySummaryDto>.Ok(summary);
    }

    public async Task<ApiResponse<AttendanceRegularizationResponseDto>> RequestRegularizationAsync(RegularizationRequestDto req, int currentUserId)
    {
        var employee = await ResolveEmployeeAsync(req.EmployeeId, currentUserId);
        if (employee == null)
            return ApiResponse<AttendanceRegularizationResponseDto>.Fail("Employee not found.");

        var reg = new AttendanceRegularization
        {
            EmployeeId = employee.Id,
            AttendanceLogId = req.AttendanceLogId,
            LogDate = req.LogDate,
            RequestedCheckIn = req.RequestedCheckIn,
            RequestedCheckOut = req.RequestedCheckOut,
            Reason = req.Reason,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        db.AttendanceRegularizations.Add(reg);
        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "REGULARIZATION_REQUEST",
            "AttendanceRegularization",
            reg.Id.ToString(),
            null,
            new { reg.Id, reg.EmployeeId, reg.LogDate, reg.Reason });

        var dto = new AttendanceRegularizationResponseDto
        {
            Id = reg.Id,
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = employee.FullName,
            AttendanceLogId = reg.AttendanceLogId,
            LogDate = reg.LogDate,
            RequestedCheckIn = reg.RequestedCheckIn,
            RequestedCheckOut = reg.RequestedCheckOut,
            Reason = reg.Reason,
            Status = reg.Status,
            CreatedAt = reg.CreatedAt
        };

        return ApiResponse<AttendanceRegularizationResponseDto>.Ok(dto, "Regularization request submitted successfully.");
    }

    public async Task<ApiResponse<PagedResult<AttendanceRegularizationResponseDto>>> GetRegularizationRequestsAsync(int? employeeId, string? status, int page, int pageSize)
    {
        var q = db.AttendanceRegularizations
            .Include(r => r.Employee)
            .Include(r => r.ReviewedByNavigation)
            .AsQueryable();

        if (employeeId.HasValue)
            q = q.Where(r => r.EmployeeId == employeeId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(r => r.Status == status);

        var total = await q.CountAsync();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var items = await q.OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(r => new AttendanceRegularizationResponseDto
        {
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            EmployeeCode = r.Employee.EmployeeCode,
            EmployeeName = r.Employee.FullName,
            AttendanceLogId = r.AttendanceLogId,
            LogDate = r.LogDate,
            RequestedCheckIn = r.RequestedCheckIn,
            RequestedCheckOut = r.RequestedCheckOut,
            Reason = r.Reason,
            Status = r.Status,
            ReviewedBy = r.ReviewedBy,
            ReviewerName = r.ReviewedByNavigation?.FullName,
            ReviewedAt = r.ReviewedAt,
            ReviewRemarks = r.ReviewRemarks,
            CreatedAt = r.CreatedAt
        }).ToList();

        var result = new PagedResult<AttendanceRegularizationResponseDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<AttendanceRegularizationResponseDto>>.Ok(result);
    }

    public async Task<ApiResponse<AttendanceRegularizationResponseDto>> ReviewRegularizationAsync(int id, RegularizationReviewDto req, int reviewerUserId)
    {
        var reg = await db.AttendanceRegularizations
            .Include(r => r.Employee)
                .ThenInclude(e => e.Shift)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reg == null)
            return ApiResponse<AttendanceRegularizationResponseDto>.Fail("Regularization request not found.");

        if (reg.Status != "Pending")
            return ApiResponse<AttendanceRegularizationResponseDto>.Fail($"Request is already {reg.Status}.");

        var reviewerEmp = await ResolveEmployeeAsync(null, reviewerUserId);

        reg.Status = req.Status;
        reg.ReviewedBy = reviewerEmp?.Id;
        reg.ReviewedAt = DateTime.UtcNow;
        reg.ReviewRemarks = req.Remarks;
        reg.UpdatedAt = DateTime.UtcNow;
        reg.UpdatedBy = reviewerUserId;

        // If Approved, update/create AttendanceLog
        if (req.Status == "Approved")
        {
            var log = await db.AttendanceLogs.FirstOrDefaultAsync(a => a.EmployeeId == reg.EmployeeId && a.LogDate == reg.LogDate);
            if (log == null)
            {
                log = new AttendanceLog
                {
                    EmployeeId = reg.EmployeeId,
                    LogDate = reg.LogDate,
                    ShiftId = reg.Employee.ShiftId,
                    CreatedAt = DateTime.UtcNow
                };
                db.AttendanceLogs.Add(log);
            }

            if (reg.RequestedCheckIn.HasValue)
                log.CheckInTime = reg.RequestedCheckIn.Value;

            if (reg.RequestedCheckOut.HasValue)
                log.CheckOutTime = reg.RequestedCheckOut.Value;

            log.Status = "Present";
            log.IsManualEntry = true;
            log.Remarks = $"Regularized: {reg.Reason}";

            if (log.CheckInTime.HasValue && log.CheckOutTime.HasValue)
            {
                var duration = log.CheckOutTime.Value - log.CheckInTime.Value;
                log.WorkingMinutes = (int)Math.Max(0, duration.TotalMinutes);
            }

            log.UpdatedAt = DateTime.UtcNow;
            log.UpdatedBy = reviewerUserId;
            reg.AttendanceLog = log;
        }

        await db.SaveChangesAsync();

        await auditService.LogAsync(
            "REGULARIZATION_REVIEW",
            "AttendanceRegularization",
            reg.Id.ToString(),
            null,
            new { reg.Id, reg.EmployeeId, Status = req.Status, req.Remarks, reviewerUserId });

        var dto = new AttendanceRegularizationResponseDto
        {
            Id = reg.Id,
            EmployeeId = reg.EmployeeId,
            EmployeeCode = reg.Employee.EmployeeCode,
            EmployeeName = reg.Employee.FullName,
            AttendanceLogId = reg.AttendanceLogId,
            LogDate = reg.LogDate,
            RequestedCheckIn = reg.RequestedCheckIn,
            RequestedCheckOut = reg.RequestedCheckOut,
            Reason = reg.Reason,
            Status = reg.Status,
            ReviewedBy = reg.ReviewedBy,
            ReviewerName = reviewerEmp?.FullName,
            ReviewedAt = reg.ReviewedAt,
            ReviewRemarks = reg.ReviewRemarks,
            CreatedAt = reg.CreatedAt
        };

        return ApiResponse<AttendanceRegularizationResponseDto>.Ok(dto, $"Regularization request {req.Status.ToLower()} successfully.");
    }

    public async Task<ApiResponse<string>> RunDailyAttendanceAggregationAsync(DateOnly date)
    {
        logger.LogInformation("Starting daily attendance aggregation for date {Date}", date);

        var activeEmployees = await db.Employees
            .Where(e => e.Status == "Active")
            .ToListAsync();

        var existingLogs = await db.AttendanceLogs
            .Where(a => a.LogDate == date)
            .Select(a => a.EmployeeId)
            .ToListAsync();

        var holidays = await db.Holidays
            .Where(h => h.HolidayDate == date)
            .ToListAsync();

        bool isHoliday = holidays.Count > 0;
        bool isWeekend = date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday;

        var approvedLeaves = await db.LeaveApplications
            .Where(l => l.StartDate <= date && l.EndDate >= date && l.Status == "Approved")
            .Select(l => l.EmployeeId)
            .ToListAsync();

        int processed = 0;
        foreach (var emp in activeEmployees)
        {
            if (existingLogs.Contains(emp.Id))
                continue;

            string status = "Absent";
            string? remarks = null;

            if (approvedLeaves.Contains(emp.Id))
            {
                status = "OnLeave";
                remarks = "Scheduled Leave";
            }
            else if (isHoliday)
            {
                status = "Holiday";
                remarks = holidays[0].Name;
            }
            else if (isWeekend)
            {
                status = "Weekend";
                remarks = "Weekly Off";
            }

            var log = new AttendanceLog
            {
                EmployeeId = emp.Id,
                LogDate = date,
                ShiftId = emp.ShiftId,
                Status = status,
                Remarks = remarks,
                IsManualEntry = false,
                CreatedAt = DateTime.UtcNow
            };

            db.AttendanceLogs.Add(log);
            processed++;
        }

        if (processed > 0)
        {
            await db.SaveChangesAsync();
        }

        logger.LogInformation("Daily attendance aggregation completed for {Date}. Processed {Count} missing employee logs.", date, processed);
        return ApiResponse<string>.Ok($"Aggregation completed for {date}. Processed {processed} employees.");
    }

    private static AttendanceLogResponseDto MapToDto(AttendanceLog a, Employee emp, Shift? shift)
    {
        return new AttendanceLogResponseDto
        {
            Id = a.Id,
            EmployeeId = a.EmployeeId,
            EmployeeCode = emp.EmployeeCode,
            EmployeeName = emp.FullName,
            DepartmentName = emp.Department?.Name,
            BranchName = emp.Branch?.Name,
            LogDate = a.LogDate,
            CheckInTime = a.CheckInTime,
            CheckOutTime = a.CheckOutTime,
            ShiftId = a.ShiftId,
            ShiftName = shift?.Name ?? a.Shift?.Name,
            Status = a.Status,
            IsLate = a.IsLate,
            LateMinutes = a.LateMinutes,
            IsEarlyLeave = a.IsEarlyLeave,
            EarlyLeaveMinutes = a.EarlyLeaveMinutes,
            OvertimeMinutes = a.OvertimeMinutes,
            WorkingMinutes = a.WorkingMinutes,
            CheckInIp = a.CheckInIp,
            CheckInLat = a.CheckInLat,
            CheckInLng = a.CheckInLng,
            CheckOutIp = a.CheckOutIp,
            IsManualEntry = a.IsManualEntry,
            Remarks = a.Remarks
        };
    }
}
