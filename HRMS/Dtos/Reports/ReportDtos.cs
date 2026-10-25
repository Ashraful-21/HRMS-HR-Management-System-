namespace HRMS.Dtos.Reports;

public class AttendanceReportItemDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public int LateDays { get; set; }
    public int AbsentDays { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public decimal AttendancePercentage { get; set; }
}

public class LeaveReportItemDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public decimal TotalAllowed { get; set; }
    public decimal UsedDays { get; set; }
    public decimal Balance { get; set; }
}

public class PayrollReportItemDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string DesignationTitle { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal GrossEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}
