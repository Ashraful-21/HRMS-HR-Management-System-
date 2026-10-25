using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class Announcement
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int? DepartmentId { get; set; }

    public int? BranchId { get; set; }

    public bool IsPinned { get; set; }

    public DateTime PublishFrom { get; set; }

    public DateTime? PublishTo { get; set; }

    public bool IsPublished { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Department? Department { get; set; }
}
