using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Common;
using HRMS.Dtos.Employees;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/employees")]
[Authorize]
[Produces("application/json")]
public class EmployeesController(IEmployeeService employeeService, ICurrentUserService currentUser) : ControllerBase
{
    // ==========================================
    // MAIN EMPLOYEE ENDPOINTS
    // ==========================================
    [HttpGet]
    [HasPermission(Permissions.EmployeesRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EmployeeSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees([FromQuery] EmployeeFilterQuery query)
    {
        var result = await employeeService.GetEmployeesPagedAsync(query);
        return Ok(result);
    }

    [HttpGet("export")]
    [HasPermission(Permissions.EmployeesExport)]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public async Task<IActionResult> ExportEmployees([FromQuery] EmployeeFilterQuery query)
    {
        var bytes = await employeeService.ExportEmployeesToExcelAsync(query);
        var filename = $"Employees_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.EmployeesRead)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployee(int id)
    {
        var result = await employeeService.GetEmployeeByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.EmployeesWrite)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDetailDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
    {
        var result = await employeeService.CreateEmployeeAsync(request, currentUser.UserId);
        return result.Success ? CreatedAtAction(nameof(GetEmployee), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.EmployeesWrite)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDetailDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request)
    {
        var result = await employeeService.UpdateEmployeeAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.EmployeesDelete)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var result = await employeeService.DeleteEmployeeAsync(id, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/photo")]
    [HasPermission(Permissions.EmployeesWrite)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadProfilePhoto(int id, IFormFile file)
    {
        var result = await employeeService.UploadProfilePhotoAsync(id, file, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ==========================================
    // SUB-ENTITIES: ADDRESSES
    // ==========================================
    [HttpPost("{id:int}/addresses")]
    [HasPermission(Permissions.EmployeesWrite)]
    public async Task<IActionResult> AddAddress(int id, [FromBody] UpsertEmployeeAddressRequest request)
    {
        var result = await employeeService.AddAddressAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}/addresses/{addressId:int}")]
    [HasPermission(Permissions.EmployeesWrite)]
    public async Task<IActionResult> DeleteAddress(int id, int addressId)
    {
        var result = await employeeService.DeleteAddressAsync(id, addressId, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ==========================================
    // SUB-ENTITIES: EMERGENCY CONTACTS
    // ==========================================
    [HttpPost("{id:int}/emergency-contacts")]
    [HasPermission(Permissions.EmployeesWrite)]
    public async Task<IActionResult> AddEmergencyContact(int id, [FromBody] UpsertEmergencyContactRequest request)
    {
        var result = await employeeService.AddEmergencyContactAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}/emergency-contacts/{contactId:int}")]
    [HasPermission(Permissions.EmployeesWrite)]
    public async Task<IActionResult> DeleteEmergencyContact(int id, int contactId)
    {
        var result = await employeeService.DeleteEmergencyContactAsync(id, contactId, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ==========================================
    // SUB-ENTITIES: BANK ACCOUNTS
    // ==========================================
    [HttpPost("{id:int}/bank-accounts")]
    [HasPermission(Permissions.EmployeesWrite)]
    public async Task<IActionResult> AddBankAccount(int id, [FromBody] UpsertBankAccountRequest request)
    {
        var result = await employeeService.AddBankAccountAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}/bank-accounts/{accountId:int}")]
    [HasPermission(Permissions.EmployeesWrite)]
    public async Task<IActionResult> DeleteBankAccount(int id, int accountId)
    {
        var result = await employeeService.DeleteBankAccountAsync(id, accountId, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ==========================================
    // SUB-ENTITIES: EDUCATION
    // ==========================================
    [HttpPost("{id:int}/educations")]
    [HasPermission(Permissions.EmployeesWrite)]
    public async Task<IActionResult> AddEducation(int id, [FromBody] UpsertEducationRequest request)
    {
        var result = await employeeService.AddEducationAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}/educations/{educationId:int}")]
    [HasPermission(Permissions.EmployeesWrite)]
    public async Task<IActionResult> DeleteEducation(int id, int educationId)
    {
        var result = await employeeService.DeleteEducationAsync(id, educationId, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ==========================================
    // SUB-ENTITIES: EXPERIENCE
    // ==========================================
    [HttpPost("{id:int}/experiences")]
    [HasPermission(Permissions.EmployeesWrite)]
    public async Task<IActionResult> AddExperience(int id, [FromBody] UpsertExperienceRequest request)
    {
        var result = await employeeService.AddExperienceAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}/experiences/{experienceId:int}")]
    [HasPermission(Permissions.EmployeesWrite)]
    public async Task<IActionResult> DeleteExperience(int id, int experienceId)
    {
        var result = await employeeService.DeleteExperienceAsync(id, experienceId, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ==========================================
    // SUB-ENTITIES: DOCUMENTS
    // ==========================================
    [HttpPost("{id:int}/documents")]
    [HasPermission(Permissions.EmployeesWrite)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocument(
        int id,
        [FromForm] string documentType,
        [FromForm] string title,
        [FromForm] bool isPrivate,
        [FromForm] DateOnly? expiryDate,
        IFormFile file)
    {
        var result = await employeeService.UploadDocumentAsync(id, documentType, title, isPrivate, expiryDate, file, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}/documents/{documentId:int}")]
    [HasPermission(Permissions.EmployeesWrite)]
    public async Task<IActionResult> DeleteDocument(int id, int documentId)
    {
        var result = await employeeService.DeleteDocumentAsync(id, documentId, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ==========================================
    // LIFECYCLE EVENTS
    // ==========================================
    [HttpPost("{id:int}/lifecycle-events")]
    [HasPermission(Permissions.EmployeesWrite)]
    public async Task<IActionResult> AddLifecycleEvent(int id, [FromBody] CreateLifecycleEventRequest request)
    {
        var result = await employeeService.AddLifecycleEventAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:int}/lifecycle-events")]
    [HasPermission(Permissions.EmployeesRead)]
    public async Task<IActionResult> GetLifecycleEvents(int id)
    {
        var result = await employeeService.GetLifecycleEventsAsync(id);
        return Ok(result);
    }

    // ==========================================
    // PORTAL LOGIN ACCOUNT & SECURITY (SUPERADMIN)
    // ==========================================
    [HttpGet("{id:int}/account")]
    [HasPermission(Permissions.EmployeesRead)]
    public async Task<IActionResult> GetPortalAccount(int id)
    {
        var result = await employeeService.GetPortalAccountAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id:int}/account")]
    [HasPermission(Permissions.UsersWrite)]
    public async Task<IActionResult> ManagePortalAccount(int id, [FromBody] ManageEmployeePortalAccountRequest request)
    {
        var result = await employeeService.ManagePortalAccountAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/reset-password")]
    [HasPermission(Permissions.UsersWrite)]
    public async Task<IActionResult> ResetEmployeePassword(int id, [FromBody] ResetEmployeePasswordRequest request)
    {
        var result = await employeeService.ResetEmployeePasswordAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
