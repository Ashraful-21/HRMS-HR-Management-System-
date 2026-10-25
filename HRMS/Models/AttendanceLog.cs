using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class AttendanceLog
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly LogDate { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public int? ShiftId { get; set; }

    public string Status { get; set; } = null!;

    public bool IsLate { get; set; }

    public int LateMinutes { get; set; }

    public bool IsEarlyLeave { get; set; }

    public int EarlyLeaveMinutes { get; set; }

    public int OvertimeMinutes { get; set; }

    public int WorkingMinutes { get; set; }

    public string? CheckInIp { get; set; }

    public decimal? CheckInLat { get; set; }

    public decimal? CheckInLng { get; set; }

    public string? CheckOutIp { get; set; }

    public bool IsManualEntry { get; set; }

    public string? Remarks { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<AttendanceRegularization> AttendanceRegularizations { get; set; } = new List<AttendanceRegularization>();

    public virtual Employee Employee { get; set; } = null!;

    public virtual Shift? Shift { get; set; }
}
