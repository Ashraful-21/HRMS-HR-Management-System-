using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class EmployeeEmergencyContact
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string Name { get; set; } = null!;

    public string Relationship { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
