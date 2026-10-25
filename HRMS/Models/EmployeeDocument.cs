using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class EmployeeDocument
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public string FilePublicId { get; set; } = null!;

    public bool IsPrivate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
