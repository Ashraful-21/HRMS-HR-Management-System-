namespace HRMS.Dtos.Dashboard;

public class DashboardSummaryDto
{
    // Workforce Stats
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int OnProbationEmployees { get; set; }
    public int ResignedEmployees { get; set; }
    public int NewHiresThisMonth { get; set; }

    // Today's Attendance
    public int AttendanceExpected { get; set; }
    public int AttendancePresent { get; set; }
    public int AttendanceLate { get; set; }
    public int AttendanceEarlyLeave { get; set; }
    public int AttendanceAbsent { get; set; }
    public decimal AttendanceRatePercent { get; set; }

    // Leave Stats
    public int PendingLeaveApplications { get; set; }
    public int OnLeaveToday { get; set; }

    // Payroll Summary (Last Run)
    public decimal LastPayrollGrossTotal { get; set; }
    public decimal LastPayrollNetTotal { get; set; }
    public string LastPayrollPeriod { get; set; } = "N/A";
    public string LastPayrollStatus { get; set; } = "N/A";

    // Open Recruitment & Assets
    public int OpenJobPositions { get; set; }
    public int TotalCandidatesInPipeline { get; set; }
    public int TotalAssetsAvailable { get; set; }
    public int TotalAssetsAssigned { get; set; }

    // Department Distribution
    public List<DepartmentDistributionItem> DepartmentDistribution { get; set; } = [];

    // Recent Activity / Audit Feed
    public List<ActivityItemDto> RecentActivities { get; set; } = [];
}

public class DepartmentDistributionItem
{
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal Percentage { get; set; }
}

public class ActivityItemDto
{
    public long Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
