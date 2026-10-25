namespace HRMS.Constants;

/// <summary>
/// All permission codes must exactly match the Permissions.Code values seeded in HRMS_schema.sql.
/// </summary>
public static class Permissions
{
    // Organisation
    public const string BranchesRead       = "branches.read";
    public const string BranchesWrite      = "branches.write";
    public const string BranchesDelete     = "branches.delete";
    public const string DepartmentsRead    = "departments.read";
    public const string DepartmentsWrite   = "departments.write";
    public const string DepartmentsDelete  = "departments.delete";
    public const string DesignationsRead   = "designations.read";
    public const string DesignationsWrite  = "designations.write";
    public const string GradesRead         = "grades.read";
    public const string GradesWrite        = "grades.write";
    public const string ShiftsRead         = "shifts.read";
    public const string ShiftsWrite        = "shifts.write";
    public const string HolidaysRead       = "holidays.read";
    public const string HolidaysWrite      = "holidays.write";

    // Employees
    public const string EmployeesRead      = "employees.read";
    public const string EmployeesWrite     = "employees.write";
    public const string EmployeesDelete    = "employees.delete";
    public const string EmployeesExport    = "employees.export";

    // Attendance
    public const string AttendanceRead      = "attendance.read";
    public const string AttendanceWrite     = "attendance.write";
    public const string AttendanceCheckIn   = "attendance.checkin";
    public const string AttendanceImport    = "attendance.import";
    public const string AttendanceRegularize = "attendance.regularize";

    // Leave
    public const string LeaveRead          = "leave.read";
    public const string LeaveApply         = "leave.apply";
    public const string LeaveApprove       = "leave.approve";
    public const string LeaveManage        = "leave.manage";

    // Payroll
    public const string PayrollRead        = "payroll.read";
    public const string PayrollProcess     = "payroll.process";
    public const string PayrollApprove     = "payroll.approve";
    public const string PayrollPayslip     = "payroll.payslip";

    // Recruitment
    public const string RecruitmentRead    = "recruitment.read";
    public const string RecruitmentWrite   = "recruitment.write";

    // Performance
    public const string PerformanceRead    = "performance.read";
    public const string PerformanceWrite   = "performance.write";

    // Assets
    public const string AssetsRead         = "assets.read";
    public const string AssetsWrite        = "assets.write";

    // Reports
    public const string ReportsRead        = "reports.read";
    public const string ReportsExport      = "reports.export";

    // Admin
    public const string UsersRead          = "users.read";
    public const string UsersWrite         = "users.write";
    public const string RolesRead          = "roles.read";
    public const string RolesWrite         = "roles.write";
    public const string AuditRead          = "audit.read";
    public const string SettingsRead       = "settings.read";
    public const string SettingsWrite      = "settings.write";

    // Communication
    public const string AnnouncementsRead  = "announcements.read";
    public const string AnnouncementsWrite = "announcements.write";
    public const string NotificationsRead  = "notifications.read";
}
