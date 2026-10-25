using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class Permission
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Module { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
