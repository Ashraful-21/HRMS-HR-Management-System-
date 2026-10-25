using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class SystemSetting
{
    public int Id { get; set; }

    public string Key { get; set; } = null!;

    public string? Value { get; set; }

    public string Category { get; set; } = null!;

    public string DataType { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsPublic { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
