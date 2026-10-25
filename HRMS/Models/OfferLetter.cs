using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class OfferLetter
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public decimal OfferedSalary { get; set; }

    public DateOnly JoiningDate { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public string? FileUrl { get; set; }

    public string? FilePublicId { get; set; }

    public string Status { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Application Application { get; set; } = null!;
}
