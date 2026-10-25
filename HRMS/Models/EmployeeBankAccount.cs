using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class EmployeeBankAccount
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string BankName { get; set; } = null!;

    public string AccountNumber { get; set; } = null!;

    public string AccountTitle { get; set; } = null!;

    public string? BranchName { get; set; }

    public string? RoutingNumber { get; set; }

    public string? SwiftCode { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
