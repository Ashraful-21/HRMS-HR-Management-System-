using System;
using System.Threading.Tasks;
using HRMS.Dtos.Common;
using HRMS.DTOs;

namespace HRMS.IServices;

public interface IAttendanceService
{
    Task<ApiResponse<AttendanceLogResponseDto>> CheckInAsync(CheckInRequest req, string clientIp, int currentUserId);
    Task<ApiResponse<AttendanceLogResponseDto>> CheckOutAsync(CheckOutRequest req, string clientIp, int currentUserId);
    Task<ApiResponse<TodayAttendanceStatusDto>> GetTodayStatusAsync(int currentUserId);
    Task<ApiResponse<AttendanceLogResponseDto>> ManualAttendanceAsync(ManualAttendanceRequest req, int currentUserId);
    Task<ApiResponse<PagedResult<AttendanceLogResponseDto>>> GetAttendanceLogsAsync(AttendanceFilterDto filter);
    Task<ApiResponse<AttendanceMonthlySummaryDto>> GetMonthlySummaryAsync(int employeeId, int year, int month);
    Task<ApiResponse<AttendanceRegularizationResponseDto>> RequestRegularizationAsync(RegularizationRequestDto req, int currentUserId);
    Task<ApiResponse<PagedResult<AttendanceRegularizationResponseDto>>> GetRegularizationRequestsAsync(int? employeeId, string? status, int page, int pageSize);
    Task<ApiResponse<AttendanceRegularizationResponseDto>> ReviewRegularizationAsync(int id, RegularizationReviewDto req, int reviewerUserId);
    Task<ApiResponse<string>> RunDailyAttendanceAggregationAsync(DateOnly date);
}
