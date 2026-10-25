namespace HRMS.Constants;

public static class AppClaimTypes
{
    public const string UserId     = "uid";
    public const string EmployeeId = "eid";
    public const string Username   = "uname";
    public const string Permission = "permission";
}

public static class EmployeeStatuses
{
    public const string Active      = "Active";
    public const string Probation   = "Probation";
    public const string Resigned    = "Resigned";
    public const string Terminated  = "Terminated";
    public const string OnLeave     = "OnLeave";
}

public static class AttendanceStatuses
{
    public const string Present     = "Present";
    public const string Absent      = "Absent";
    public const string Late        = "Late";
    public const string HalfDay     = "HalfDay";
    public const string OnLeave     = "OnLeave";
    public const string Holiday     = "Holiday";
    public const string Weekend     = "Weekend";
    public const string EarlyLeave  = "EarlyLeave";
}

public static class LeaveStatuses
{
    public const string Pending     = "Pending";
    public const string Approved    = "Approved";
    public const string Rejected    = "Rejected";
    public const string Cancelled   = "Cancelled";
}

public static class PayrollStatuses
{
    public const string Draft       = "Draft";
    public const string Processed   = "Processed";
    public const string Approved    = "Approved";
    public const string Paid        = "Paid";
    public const string Locked      = "Locked";
}

public static class ApplicationStages
{
    public const string Applied     = "Applied";
    public const string Screening   = "Screening";
    public const string Interview   = "Interview";
    public const string Offered     = "Offered";
    public const string Hired       = "Hired";
    public const string Rejected    = "Rejected";
}

public static class AuditActions
{
    public const string Create      = "Create";
    public const string Update      = "Update";
    public const string Delete      = "Delete";
    public const string Login       = "Login";
    public const string Logout      = "Logout";
    public const string FailedLogin = "FailedLogin";
    public const string Export      = "Export";
    public const string PasswordReset = "PasswordReset";
}
