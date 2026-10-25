using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class VwEmployeeDirectory
{
    public int Id { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public string WorkEmail { get; set; } = null!;

    public string? PersonalPhone { get; set; }

    public string? WorkPhone { get; set; }

    public string? ProfilePhotoUrl { get; set; }

    public string Status { get; set; } = null!;

    public string EmploymentType { get; set; } = null!;

    public DateOnly JoiningDate { get; set; }

    public DateOnly? ProbationEndDate { get; set; }

    public DateOnly? ConfirmationDate { get; set; }

    public string? Department { get; set; }

    public string? Designation { get; set; }

    public string? Branch { get; set; }

    public string? Grade { get; set; }

    public string? Shift { get; set; }

    public string ManagerName { get; set; } = null!;

    public int? ManagerId { get; set; }
}
