using HRMS.Dtos.Common;
using HRMS.Dtos.Organization;

namespace HRMS.IServices;

public interface IOrganizationService
{
    // Branches
    Task<ApiResponse<PagedResult<BranchDto>>> GetBranchesAsync(PagedQuery query);
    Task<ApiResponse<List<BranchDto>>> GetAllBranchesAsync();
    Task<ApiResponse<BranchDto>> GetBranchByIdAsync(int id);
    Task<ApiResponse<BranchDto>> CreateBranchAsync(CreateBranchRequest request, int? userId);
    Task<ApiResponse<BranchDto>> UpdateBranchAsync(int id, UpdateBranchRequest request, int? userId);
    Task<ApiResponse> DeleteBranchAsync(int id, int? userId);

    // Departments
    Task<ApiResponse<List<DepartmentDto>>> GetDepartmentsAsync(int? branchId);
    Task<ApiResponse<List<DepartmentTreeNodeDto>>> GetDepartmentTreeAsync(int? branchId);
    Task<ApiResponse<DepartmentDto>> GetDepartmentByIdAsync(int id);
    Task<ApiResponse<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentRequest request, int? userId);
    Task<ApiResponse<DepartmentDto>> UpdateDepartmentAsync(int id, UpdateDepartmentRequest request, int? userId);
    Task<ApiResponse> DeleteDepartmentAsync(int id, int? userId);

    // Designations
    Task<ApiResponse<List<DesignationDto>>> GetDesignationsAsync(int? departmentId);
    Task<ApiResponse<DesignationDto>> GetDesignationByIdAsync(int id);
    Task<ApiResponse<DesignationDto>> CreateDesignationAsync(CreateDesignationRequest request, int? userId);
    Task<ApiResponse<DesignationDto>> UpdateDesignationAsync(int id, UpdateDesignationRequest request, int? userId);
    Task<ApiResponse> DeleteDesignationAsync(int id, int? userId);

    // Grades
    Task<ApiResponse<List<GradeDto>>> GetGradesAsync();
    Task<ApiResponse<GradeDto>> GetGradeByIdAsync(int id);
    Task<ApiResponse<GradeDto>> CreateGradeAsync(CreateGradeRequest request, int? userId);
    Task<ApiResponse<GradeDto>> UpdateGradeAsync(int id, UpdateGradeRequest request, int? userId);
    Task<ApiResponse> DeleteGradeAsync(int id, int? userId);

    // Shifts
    Task<ApiResponse<List<ShiftDto>>> GetShiftsAsync();
    Task<ApiResponse<ShiftDto>> GetShiftByIdAsync(int id);
    Task<ApiResponse<ShiftDto>> CreateShiftAsync(CreateShiftRequest request, int? userId);
    Task<ApiResponse<ShiftDto>> UpdateShiftAsync(int id, UpdateShiftRequest request, int? userId);
    Task<ApiResponse> DeleteShiftAsync(int id, int? userId);

    // Holidays
    Task<ApiResponse<List<HolidayDto>>> GetHolidaysAsync(int? year, int? branchId);
    Task<ApiResponse<HolidayDto>> GetHolidayByIdAsync(int id);
    Task<ApiResponse<HolidayDto>> CreateHolidayAsync(CreateHolidayRequest request, int? userId);
    Task<ApiResponse<HolidayDto>> UpdateHolidayAsync(int id, UpdateHolidayRequest request, int? userId);
    Task<ApiResponse> DeleteHolidayAsync(int id, int? userId);
}
