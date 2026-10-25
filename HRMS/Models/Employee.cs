using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class Employee
{
    public int Id { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public string? NationalId { get; set; }

    public string? BloodGroup { get; set; }

    public string? PersonalEmail { get; set; }

    public string WorkEmail { get; set; } = null!;

    public string? PersonalPhone { get; set; }

    public string? WorkPhone { get; set; }

    public string? MaritalStatus { get; set; }

    public string? Nationality { get; set; }

    public string? Religion { get; set; }

    public string? ProfilePhotoUrl { get; set; }

    public string? ProfilePhotoPublicId { get; set; }

    public int BranchId { get; set; }

    public int DepartmentId { get; set; }

    public int DesignationId { get; set; }

    public int? GradeId { get; set; }

    public int? ShiftId { get; set; }

    public int? ReportingManagerId { get; set; }

    public DateOnly JoiningDate { get; set; }

    public DateOnly? ProbationEndDate { get; set; }

    public DateOnly? ConfirmationDate { get; set; }

    public string EmploymentType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateOnly? ResignationDate { get; set; }

    public DateOnly? LastWorkingDate { get; set; }

    public string? TerminationReason { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<AssetAssignment> AssetAssignments { get; set; } = new List<AssetAssignment>();

    public virtual ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();

    public virtual ICollection<AttendanceRegularization> AttendanceRegularizationEmployees { get; set; } = new List<AttendanceRegularization>();

    public virtual ICollection<AttendanceRegularization> AttendanceRegularizationReviewedByNavigations { get; set; } = new List<AttendanceRegularization>();

    public virtual Branch Branch { get; set; } = null!;

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual Designation Designation { get; set; } = null!;

    public virtual ICollection<EmployeeAddress> EmployeeAddresses { get; set; } = new List<EmployeeAddress>();

    public virtual ICollection<EmployeeBankAccount> EmployeeBankAccounts { get; set; } = new List<EmployeeBankAccount>();

    public virtual ICollection<EmployeeDocument> EmployeeDocuments { get; set; } = new List<EmployeeDocument>();

    public virtual ICollection<EmployeeEducation> EmployeeEducations { get; set; } = new List<EmployeeEducation>();

    public virtual ICollection<EmployeeEmergencyContact> EmployeeEmergencyContacts { get; set; } = new List<EmployeeEmergencyContact>();

    public virtual ICollection<EmployeeExperience> EmployeeExperiences { get; set; } = new List<EmployeeExperience>();

    public virtual ICollection<EmployeeLifecycleEvent> EmployeeLifecycleEvents { get; set; } = new List<EmployeeLifecycleEvent>();

    public virtual Grade? Grade { get; set; }

    public virtual ICollection<Interview> Interviews { get; set; } = new List<Interview>();

    public virtual ICollection<Employee> InverseReportingManager { get; set; } = new List<Employee>();

    public virtual ICollection<LeaveApplication> LeaveApplicationApprovedByNavigations { get; set; } = new List<LeaveApplication>();

    public virtual ICollection<LeaveApplication> LeaveApplicationEmployees { get; set; } = new List<LeaveApplication>();

    public virtual ICollection<LeaveApplication> LeaveApplicationRejectedByNavigations { get; set; } = new List<LeaveApplication>();

    public virtual ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();

    public virtual ICollection<Loan> LoanApprovedByNavigations { get; set; } = new List<Loan>();

    public virtual ICollection<Loan> LoanEmployees { get; set; } = new List<Loan>();

    public virtual ICollection<PayrollDetail> PayrollDetails { get; set; } = new List<PayrollDetail>();

    public virtual ICollection<PerformanceReview> PerformanceReviewEmployees { get; set; } = new List<PerformanceReview>();

    public virtual ICollection<PerformanceReview> PerformanceReviewReviewers { get; set; } = new List<PerformanceReview>();

    public virtual Employee? ReportingManager { get; set; }

    public virtual ICollection<SalaryStructure> SalaryStructures { get; set; } = new List<SalaryStructure>();

    public virtual Shift? Shift { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
