using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class EmployeeAddress
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string AddressType { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string City { get; set; } = null!;

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public string Country { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
