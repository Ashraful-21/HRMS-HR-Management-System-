using HRMS.Data;
using HRMS.Dtos.Common;
using HRMS.Dtos.Dashboard;
using HRMS.Dtos.Reports;
using HRMS.IServices;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Services;

public class DashboardService(HrmsDbContext db, ILogger<DashboardService> logger) : IDashboardService
{
    public async Task<ApiResponse<DashboardSummaryDto>> GetDashboardSummaryAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var firstDayOfMonth = new DateOnly(today.Year, today.Month, 1);

        // 1. Workforce Stats
        var employees = await db.Employees
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Select(e => new { e.Id, e.Status, e.JoiningDate, e.DepartmentId, DepartmentName = e.Department.Name })
            .ToListAsync();

        var total = employees.Count;
        var active = employees.Count(e => e.Status == "Active");
        var probation = employees.Count(e => e.Status == "Probation");
        var resigned = employees.Count(e => e.Status == "Resigned" || e.Status == "Terminated");
        var newHires = employees.Count(e => e.JoiningDate >= firstDayOfMonth);

        // Department distribution
        var deptGroups = employees
            .GroupBy(e => e.DepartmentName)
            .Select(g => new DepartmentDistributionItem
            {
                DepartmentName = g.Key,
                EmployeeCount = g.Count(),
                Percentage = total > 0 ? Math.Round((decimal)g.Count() / total * 100, 1) : 0
            })
            .OrderByDescending(d => d.EmployeeCount)
            .ToList();

        // 2. Attendance Stats for Today
        var attendanceToday = await db.AttendanceLogs
            .AsNoTracking()
            .Where(a => a.LogDate == today && !a.IsDeleted)
            .ToListAsync();

        var present = attendanceToday.Count(a => a.Status == "Present");
        var late = attendanceToday.Count(a => a.IsLate);
        var earlyLeave = attendanceToday.Count(a => a.IsEarlyLeave);
        var expected = total;
        var absent = Math.Max(0, expected - present);
        var attRate = expected > 0 ? Math.Round((decimal)present / expected * 100, 1) : 0;

        // 3. Leave Stats
        var pendingLeaves = await db.LeaveApplications
            .AsNoTracking()
            .CountAsync(l => l.Status == "Pending" && !l.IsDeleted);

        var onLeaveToday = await db.LeaveApplications
            .AsNoTracking()
            .CountAsync(l => l.Status == "Approved" && l.StartDate <= today && l.EndDate >= today && !l.IsDeleted);

        // 4. Payroll Stats
        var lastRun = await db.PayrollRuns
            .AsNoTracking()
            .Include(p => p.PayrollDetails)
            .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
            .FirstOrDefaultAsync(p => !p.IsDeleted);

        decimal grossTotal = 0;
        decimal netTotal = 0;
        var period = "No payroll run yet";
        var status = "Draft";

        if (lastRun != null)
        {
            grossTotal = lastRun.PayrollDetails.Sum(d => d.GrossEarnings);
            netTotal = lastRun.PayrollDetails.Sum(d => d.NetPay);
            period = $"{lastRun.Month:D2}/{lastRun.Year}";
            status = lastRun.Status;
        }

        // 5. Recruitment & Assets
        var openJobs = await db.JobOpenings.AsNoTracking().CountAsync(j => j.Status == "Open" && !j.IsDeleted);
        var candidatesPipeline = await db.Applications.AsNoTracking().CountAsync(a => a.Stage != "Hired" && a.Stage != "Rejected" && !a.IsDeleted);
        var assetsAvailable = await db.Assets.AsNoTracking().CountAsync(a => a.Status == "Available" && !a.IsDeleted);
        var assetsAssigned = await db.Assets.AsNoTracking().CountAsync(a => a.Status == "Assigned" && !a.IsDeleted);

        // 6. Recent Audit Activities
        var recentLogs = await db.AuditLogs
            .AsNoTracking()
            .OrderByDescending(a => a.Timestamp)
            .Take(7)
            .Select(a => new ActivityItemDto
            {
                Id = a.Id,
                Action = a.Action,
                EntityType = a.EntityType ?? "System",
                Username = a.Username ?? "System",
                Timestamp = a.Timestamp
            })
            .ToListAsync();

        return ApiResponse<DashboardSummaryDto>.Ok(new DashboardSummaryDto
        {
            TotalEmployees = total,
            ActiveEmployees = active,
            OnProbationEmployees = probation,
            ResignedEmployees = resigned,
            NewHiresThisMonth = newHires,
            AttendanceExpected = expected,
            AttendancePresent = present,
            AttendanceLate = late,
            AttendanceEarlyLeave = earlyLeave,
            AttendanceAbsent = absent,
            AttendanceRatePercent = attRate,
            PendingLeaveApplications = pendingLeaves,
            OnLeaveToday = onLeaveToday,
            LastPayrollGrossTotal = grossTotal,
            LastPayrollNetTotal = netTotal,
            LastPayrollPeriod = period,
            LastPayrollStatus = status,
            OpenJobPositions = openJobs,
            TotalCandidatesInPipeline = candidatesPipeline,
            TotalAssetsAvailable = assetsAvailable,
            TotalAssetsAssigned = assetsAssigned,
            DepartmentDistribution = deptGroups,
            RecentActivities = recentLogs
        });
    }

    public async Task<ApiResponse<List<AttendanceReportItemDto>>> GetAttendanceReportAsync(int month, int year, int? departmentId = null)
    {
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var q = db.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.AttendanceLogs.Where(a => a.LogDate >= startDate && a.LogDate <= endDate && !a.IsDeleted))
            .Where(e => !e.IsDeleted);

        if (departmentId.HasValue)
            q = q.Where(e => e.DepartmentId == departmentId.Value);

        var emps = await q.ToListAsync();
        var daysInMonth = DateTime.DaysInMonth(year, month);

        var list = emps.Select(e =>
        {
            var presentDays = e.AttendanceLogs.Count(a => a.Status == "Present");
            var lateDays = e.AttendanceLogs.Count(a => a.IsLate);
            var absentDays = e.AttendanceLogs.Count(a => a.Status == "Absent");
            var otMinutes = e.AttendanceLogs.Sum(a => a.OvertimeMinutes);

            return new AttendanceReportItemDto
            {
                EmployeeId = e.Id,
                EmployeeCode = e.EmployeeCode,
                FullName = e.FirstName + " " + e.LastName,
                DepartmentName = e.Department.Name,
                TotalDays = daysInMonth,
                PresentDays = presentDays,
                LateDays = lateDays,
                AbsentDays = absentDays,
                TotalOvertimeHours = Math.Round((decimal)otMinutes / 60, 2),
                AttendancePercentage = daysInMonth > 0 ? Math.Round((decimal)presentDays / daysInMonth * 100, 1) : 0
            };
        }).ToList();

        return ApiResponse<List<AttendanceReportItemDto>>.Ok(list);
    }

    public async Task<ApiResponse<List<LeaveReportItemDto>>> GetLeaveSummaryReportAsync(int year, int? departmentId = null)
    {
        var q = db.LeaveBalances
            .AsNoTracking()
            .Include(lb => lb.Employee).ThenInclude(e => e.Department)
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.Year == year && !lb.Employee.IsDeleted);

        if (departmentId.HasValue)
            q = q.Where(lb => lb.Employee.DepartmentId == departmentId.Value);

        var list = await q.Select(lb => new LeaveReportItemDto
        {
            EmployeeId = lb.EmployeeId,
            EmployeeCode = lb.Employee.EmployeeCode,
            FullName = lb.Employee.FirstName + " " + lb.Employee.LastName,
            DepartmentName = lb.Employee.Department.Name,
            LeaveType = lb.LeaveType.Name,
            TotalAllowed = lb.TotalDays + lb.CarryForwardDays,
            UsedDays = lb.UsedDays,
            Balance = lb.Balance ?? 0
        }).ToListAsync();

        return ApiResponse<List<LeaveReportItemDto>>.Ok(list);
    }

    public async Task<ApiResponse<List<PayrollReportItemDto>>> GetPayrollSummaryReportAsync(int month, int year, int? branchId = null)
    {
        var q = db.PayrollDetails
            .AsNoTracking()
            .Include(pd => pd.Employee).ThenInclude(e => e.Department)
            .Include(pd => pd.Employee).ThenInclude(e => e.Designation)
            .Include(pd => pd.PayrollRun)
            .Where(pd => pd.PayrollRun.Month == month && pd.PayrollRun.Year == year && !pd.PayrollRun.IsDeleted);

        if (branchId.HasValue)
            q = q.Where(pd => pd.PayrollRun.BranchId == branchId.Value);

        var list = await q.Select(pd => new PayrollReportItemDto
        {
            EmployeeId = pd.EmployeeId,
            EmployeeCode = pd.Employee.EmployeeCode,
            FullName = pd.Employee.FirstName + " " + pd.Employee.LastName,
            DepartmentName = pd.Employee.Department.Name,
            DesignationTitle = pd.Employee.Designation.Title,
            BasicSalary = pd.GrossEarnings,
            GrossEarnings = pd.GrossEarnings,
            TotalDeductions = pd.TotalDeductions,
            NetPay = pd.NetPay,
            Month = pd.PayrollRun.Month,
            Year = pd.PayrollRun.Year
        }).ToListAsync();

        return ApiResponse<List<PayrollReportItemDto>>.Ok(list);
    }
}
