using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.Dtos.Common;
using HRMS.DTOs;

namespace HRMS.IServices;

public interface ILeaveService
{
    // Leave Types
    Task<ApiResponse<List<LeaveTypeDto>>> GetLeaveTypesAsync();
    Task<ApiResponse<LeaveTypeDto>> CreateLeaveTypeAsync(CreateLeaveTypeRequest req, int currentUserId);
    Task<ApiResponse<LeaveTypeDto>> UpdateLeaveTypeAsync(int id, UpdateLeaveTypeRequest req, int currentUserId);
    Task<ApiResponse<string>> DeleteLeaveTypeAsync(int id, int currentUserId);

    // Leave Balances
    Task<ApiResponse<List<LeaveBalanceDto>>> GetEmployeeLeaveBalancesAsync(int employeeId, int? year);
    Task<ApiResponse<string>> AllocateLeaveBalanceAsync(AllocateLeaveBalanceRequest req, int currentUserId);
    Task<ApiResponse<string>> ProcessYearEndCarryForwardAsync(YearEndCarryForwardRequest req, int currentUserId);

    // Leave Applications
    Task<ApiResponse<LeaveApplicationDto>> ApplyLeaveAsync(ApplyLeaveRequest req, int currentUserId);
    Task<ApiResponse<PagedResult<LeaveApplicationDto>>> GetLeaveApplicationsAsync(LeaveApplicationFilterDto filter, int currentUserId);
    Task<ApiResponse<LeaveApplicationDto>> GetLeaveApplicationByIdAsync(int id);
    Task<ApiResponse<LeaveApplicationDto>> ReviewLeaveApplicationAsync(int id, ReviewLeaveApplicationRequest req, int reviewerUserId);
    Task<ApiResponse<string>> CancelLeaveApplicationAsync(int id, int currentUserId);
}
