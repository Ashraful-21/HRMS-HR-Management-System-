using HRMS.Dtos.Common;
using HRMS.Dtos.Employees;

namespace HRMS.IServices;

public interface IEmployeeService
{
    // Employee Main
    Task<ApiResponse<PagedResult<EmployeeSummaryDto>>> GetEmployeesPagedAsync(EmployeeFilterQuery query);
    Task<ApiResponse<EmployeeDetailDto>> GetEmployeeByIdAsync(int id);
    Task<ApiResponse<EmployeeDetailDto>> CreateEmployeeAsync(CreateEmployeeRequest request, int? userId);
    Task<ApiResponse<EmployeeDetailDto>> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request, int? userId);
    Task<ApiResponse> DeleteEmployeeAsync(int id, int? userId);
    Task<ApiResponse<string>> UploadProfilePhotoAsync(int id, IFormFile file, int? userId);

    // Sub-entities
    Task<ApiResponse<EmployeeAddressDto>> AddAddressAsync(int employeeId, UpsertEmployeeAddressRequest request, int? userId);
    Task<ApiResponse> DeleteAddressAsync(int employeeId, int addressId, int? userId);

    Task<ApiResponse<EmployeeEmergencyContactDto>> AddEmergencyContactAsync(int employeeId, UpsertEmergencyContactRequest request, int? userId);
    Task<ApiResponse> DeleteEmergencyContactAsync(int employeeId, int contactId, int? userId);

    Task<ApiResponse<EmployeeBankAccountDto>> AddBankAccountAsync(int employeeId, UpsertBankAccountRequest request, int? userId);
    Task<ApiResponse> DeleteBankAccountAsync(int employeeId, int accountId, int? userId);

    Task<ApiResponse<EmployeeEducationDto>> AddEducationAsync(int employeeId, UpsertEducationRequest request, int? userId);
    Task<ApiResponse> DeleteEducationAsync(int employeeId, int educationId, int? userId);

    Task<ApiResponse<EmployeeExperienceDto>> AddExperienceAsync(int employeeId, UpsertExperienceRequest request, int? userId);
    Task<ApiResponse> DeleteExperienceAsync(int employeeId, int experienceId, int? userId);

    Task<ApiResponse<EmployeeDocumentDto>> UploadDocumentAsync(int employeeId, string documentType, string title, bool isPrivate, DateOnly? expiryDate, IFormFile file, int? userId);
    Task<ApiResponse> DeleteDocumentAsync(int employeeId, int documentId, int? userId);

    // Lifecycle Events
    Task<ApiResponse<EmployeeLifecycleEventDto>> AddLifecycleEventAsync(int employeeId, CreateLifecycleEventRequest request, int? userId);
    Task<ApiResponse<List<EmployeeLifecycleEventDto>>> GetLifecycleEventsAsync(int employeeId);

    // Excel Export
    Task<byte[]> ExportEmployeesToExcelAsync(EmployeeFilterQuery query);

    // Portal Login Account Management
    Task<ApiResponse<EmployeePortalAccountDto>> GetPortalAccountAsync(int employeeId);
    Task<ApiResponse<EmployeePortalAccountDto>> ManagePortalAccountAsync(int employeeId, ManageEmployeePortalAccountRequest request, int? currentUserId);
    Task<ApiResponse> ResetEmployeePasswordAsync(int employeeId, ResetEmployeePasswordRequest request, int? currentUserId);
}
