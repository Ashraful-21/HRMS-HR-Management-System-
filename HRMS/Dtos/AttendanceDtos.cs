using System;

namespace HRMS.DTOs;

public class CheckInRequest
{
    public int? EmployeeId { get; set; }
    public DateTime? CheckInTime { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Remarks { get; set; }
}

public class CheckOutRequest
{
    public int? EmployeeId { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string? Remarks { get; set; }
}

public class ManualAttendanceRequest
{
    public int EmployeeId { get; set; }
    public DateOnly LogDate { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public int? ShiftId { get; set; }
    public string Status { get; set; } = "Present";
    public string? Remarks { get; set; }
}

public class AttendanceFilterDto
{
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public int? BranchId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AttendanceLogResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? BranchName { get; set; }
    public DateOnly LogDate { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public int? ShiftId { get; set; }
    public string? ShiftName { get; set; }
    public string Status { get; set; } = "Present";
    public bool IsLate { get; set; }
    public int LateMinutes { get; set; }
    public bool IsEarlyLeave { get; set; }
    public int EarlyLeaveMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public int WorkingMinutes { get; set; }
    public string? CheckInIp { get; set; }
    public decimal? CheckInLat { get; set; }
    public decimal? CheckInLng { get; set; }
    public string? CheckOutIp { get; set; }
    public bool IsManualEntry { get; set; }
    public string? Remarks { get; set; }
}

public class RegularizationRequestDto
{
    public int? EmployeeId { get; set; }
    public int? AttendanceLogId { get; set; }
    public DateOnly LogDate { get; set; }
    public DateTime? RequestedCheckIn { get; set; }
    public DateTime? RequestedCheckOut { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class RegularizationReviewDto
{
    public string Status { get; set; } = "Approved"; // "Approved" or "Rejected"
    public string? Remarks { get; set; }
}

public class AttendanceRegularizationResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int? AttendanceLogId { get; set; }
    public DateOnly LogDate { get; set; }
    public DateTime? RequestedCheckIn { get; set; }
    public DateTime? RequestedCheckOut { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public int? ReviewedBy { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewRemarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TodayAttendanceStatusDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateOnly LogDate { get; set; }
    public bool HasCheckedIn { get; set; }
    public bool HasCheckedOut { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public int WorkingMinutes { get; set; }
    public string Status { get; set; } = "Absent";
    public bool IsLate { get; set; }
    public int LateMinutes { get; set; }
    public string? ShiftName { get; set; }
    public TimeOnly? ShiftStartTime { get; set; }
    public TimeOnly? ShiftEndTime { get; set; }
    public int ShiftGraceMinutes { get; set; }
}

public class AttendanceMonthlySummaryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalDaysInMonth { get; set; }
    public int PresentDays { get; set; }
    public int LateDays { get; set; }
    public int EarlyLeaveDays { get; set; }
    public int HalfDays { get; set; }
    public int AbsentDays { get; set; }
    public int HolidayDays { get; set; }
    public int WeekendDays { get; set; }
    public int OnLeaveDays { get; set; }
    public int TotalWorkingMinutes { get; set; }
    public int TotalOvertimeMinutes { get; set; }
}
