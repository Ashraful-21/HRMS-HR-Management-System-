using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class Asset
{
    public int Id { get; set; }

    public string AssetCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int CategoryId { get; set; }

    public string? SerialNumber { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    public decimal? PurchaseCost { get; set; }

    public string Condition { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<AssetAssignment> AssetAssignments { get; set; } = new List<AssetAssignment>();

    public virtual AssetCategory Category { get; set; } = null!;
}
