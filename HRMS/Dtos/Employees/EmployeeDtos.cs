using HRMS.Dtos.Common;

namespace HRMS.Dtos.Employees;

public class EmployeeFilterQuery : PagedQuery
{
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public string? Status { get; set; }
    public DateOnly? JoiningFrom { get; set; }
    public DateOnly? JoiningTo { get; set; }
}

public class CreateEmployeeRequest
{
    // Personal details
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string? NationalId { get; set; }
    public string? BloodGroup { get; set; }
    public string? PersonalEmail { get; set; }
    public string WorkEmail { get; set; } = string.Empty;
    public string? PersonalPhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Nationality { get; set; }
    public string? Religion { get; set; }

    // Job details
    public int BranchId { get; set; }
    public int DepartmentId { get; set; }
    public int DesignationId { get; set; }
    public int? GradeId { get; set; }
    public int? ShiftId { get; set; }
    public int? ReportingManagerId { get; set; }
    public DateOnly JoiningDate { get; set; }
    public DateOnly? ProbationEndDate { get; set; }
    public string EmploymentType { get; set; } = "FullTime"; // FullTime, PartTime, Contract, Intern
    public string Status { get; set; } = "Probation";        // Active, Probation, Resigned, Terminated

    // Optional portal login creation
    public bool CreatePortalAccount { get; set; } = false;
    public string? PortalUsername { get; set; }
    public string? PortalPassword { get; set; }
    public string? PortalRole { get; set; } = "Employee";
}

public class UpdateEmployeeRequest : CreateEmployeeRequest
{
    public DateOnly? ConfirmationDate { get; set; }
    public DateOnly? ResignationDate { get; set; }
    public DateOnly? LastWorkingDate { get; set; }
    public string? TerminationReason { get; set; }
}

public class EmployeeSummaryDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string WorkEmail { get; set; } = string.Empty;
    public string? WorkPhone { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int DesignationId { get; set; }
    public string DesignationTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public DateOnly JoiningDate { get; set; }
    public string? ProfilePhotoUrl { get; set; }
}

public class EmployeeDetailDto : EmployeeSummaryDto
{
    public DateOnly DateOfBirth { get; set; }
    public string? NationalId { get; set; }
    public string? BloodGroup { get; set; }
    public string? PersonalEmail { get; set; }
    public string? PersonalPhone { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Nationality { get; set; }
    public string? Religion { get; set; }

    public int? GradeId { get; set; }
    public string? GradeName { get; set; }
    public int? ShiftId { get; set; }
    public string? ShiftName { get; set; }
    public int? ReportingManagerId { get; set; }
    public string? ReportingManagerName { get; set; }

    public DateOnly? ProbationEndDate { get; set; }
    public DateOnly? ConfirmationDate { get; set; }
    public DateOnly? ResignationDate { get; set; }
    public DateOnly? LastWorkingDate { get; set; }
    public string? TerminationReason { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<EmployeeAddressDto> Addresses { get; set; } = [];
    public List<EmployeeEmergencyContactDto> EmergencyContacts { get; set; } = [];
    public List<EmployeeBankAccountDto> BankAccounts { get; set; } = [];
    public List<EmployeeEducationDto> Educations { get; set; } = [];
    public List<EmployeeExperienceDto> Experiences { get; set; } = [];
    public List<EmployeeDocumentDto> Documents { get; set; } = [];
    public List<EmployeeLifecycleEventDto> LifecycleEvents { get; set; } = [];
    public EmployeePortalAccountDto? PortalAccount { get; set; }
}

public class EmployeePortalAccountDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = [];
}

public class ManageEmployeePortalAccountRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string Role { get; set; } = "Employee";
    public bool IsActive { get; set; } = true;
}

public class ResetEmployeePasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}

// Sub-entity DTOs
public class UpsertEmployeeAddressRequest
{
    public string AddressType { get; set; } = "Present"; // Present, Permanent, Office
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;
}

public class EmployeeAddressDto : UpsertEmployeeAddressRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
}

public class UpsertEmergencyContactRequest
{
    public string Name { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsPrimary { get; set; }
}

public class EmployeeEmergencyContactDto : UpsertEmergencyContactRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
}

public class UpsertBankAccountRequest
{
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountTitle { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string? RoutingNumber { get; set; }
    public string? SwiftCode { get; set; }
    public bool IsPrimary { get; set; } = true;
}

public class EmployeeBankAccountDto : UpsertBankAccountRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
}

public class UpsertEducationRequest
{
    public string Degree { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public string? FieldOfStudy { get; set; }
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public string? Grade { get; set; }
    public bool IsHighest { get; set; }
}

public class EmployeeEducationDto : UpsertEducationRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
}

public class UpsertExperienceRequest
{
    public string Company { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? Description { get; set; }
}

public class EmployeeExperienceDto : UpsertExperienceRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
}

public class EmployeeDocumentDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateLifecycleEventRequest
{
    public string EventType { get; set; } = string.Empty; // Onboarding, Promotion, Transfer, SalaryRevision, Resignation, Termination
    public DateOnly EventDate { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Remarks { get; set; }
}

public class EmployeeLifecycleEventDto : CreateLifecycleEventRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime CreatedAt { get; set; }
}
