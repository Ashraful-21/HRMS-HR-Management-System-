using System;
using System.Collections.Generic;

namespace HRMS.DTOs;

public class LeaveTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal DaysAllowed { get; set; }
    public bool IsCarryForwardAllowed { get; set; }
    public decimal? MaxCarryForwardDays { get; set; }
    public bool IsPaidLeave { get; set; }
    public bool IsHalfDayAllowed { get; set; }
    public bool IsAttachmentRequired { get; set; }
    public int? AttachmentRequiredAfterDays { get; set; }
    public string? GenderRestriction { get; set; }
    public bool IsActive { get; set; }
}

public class CreateLeaveTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal DaysAllowed { get; set; }
    public bool IsCarryForwardAllowed { get; set; }
    public decimal? MaxCarryForwardDays { get; set; }
    public bool IsPaidLeave { get; set; } = true;
    public bool IsHalfDayAllowed { get; set; } = true;
    public bool IsAttachmentRequired { get; set; }
    public int? AttachmentRequiredAfterDays { get; set; }
    public string? GenderRestriction { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateLeaveTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal DaysAllowed { get; set; }
    public bool IsCarryForwardAllowed { get; set; }
    public decimal? MaxCarryForwardDays { get; set; }
    public bool IsPaidLeave { get; set; }
    public bool IsHalfDayAllowed { get; set; }
    public bool IsAttachmentRequired { get; set; }
    public int? AttachmentRequiredAfterDays { get; set; }
    public string? GenderRestriction { get; set; }
    public bool IsActive { get; set; }
}

public class LeaveBalanceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveTypeCode { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalDays { get; set; }
    public decimal CarryForwardDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal Balance { get; set; }
}

public class AllocateLeaveBalanceRequest
{
    public int? EmployeeId { get; set; } // null to allocate to all active employees
    public int LeaveTypeId { get; set; }
    public int Year { get; set; }
    public decimal Days { get; set; }
    public decimal CarryForwardDays { get; set; }
}

public class YearEndCarryForwardRequest
{
    public int FromYear { get; set; }
    public int ToYear { get; set; }
}

public class ApplyLeaveRequest
{
    public int? EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsHalfDay { get; set; }
    public string? HalfDayType { get; set; } // "FirstHalf" | "SecondHalf"
    public string Reason { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
}

public class ReviewLeaveApplicationRequest
{
    public string Status { get; set; } = "Approved"; // "Approved" | "Rejected"
    public string? Remarks { get; set; }
}

public class LeaveApplicationDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveTypeCode { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsHalfDay { get; set; }
    public string? HalfDayType { get; set; }
    public decimal TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? AttachmentUrl { get; set; }
    public int? ApprovedBy { get; set; }
    public string? ApproverName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? RejectedBy { get; set; }
    public string? RejecterName { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? ApprovalRemarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LeaveApplicationFilterDto
{
    public int? EmployeeId { get; set; }
    public int? LeaveTypeId { get; set; }
    public string? Status { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
