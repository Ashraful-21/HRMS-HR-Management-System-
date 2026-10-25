using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class AssetCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
