namespace HRMS.Dtos.Organization;

// --- BRANCHES ---
public class CreateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsHeadOffice { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateBranchRequest : CreateBranchRequest { }

public class BranchDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsHeadOffice { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DepartmentCount { get; set; }
    public int EmployeeCount { get; set; }
}

// --- DEPARTMENTS ---
public class CreateDepartmentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public int? ParentId { get; set; }
    public int? HeadEmployeeId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateDepartmentRequest : CreateDepartmentRequest { }

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int? HeadEmployeeId { get; set; }
    public string? HeadEmployeeName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DepartmentTreeNodeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string? HeadEmployeeName { get; set; }
    public List<DepartmentTreeNodeDto> Children { get; set; } = [];
}

// --- DESIGNATIONS ---
public class CreateDesignationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateDesignationRequest : CreateDesignationRequest { }

public class DesignationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int EmployeeCount { get; set; }
}

// --- GRADES ---
public class CreateGradeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateGradeRequest : CreateGradeRequest { }

public class GradeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// --- SHIFTS ---
public class CreateShiftRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int GraceMinutes { get; set; } = 15;
    public bool IsNightShift { get; set; }
    public decimal WorkingHours { get; set; } = 8.0m;
    public bool IsActive { get; set; } = true;
}

public class UpdateShiftRequest : CreateShiftRequest { }

public class ShiftDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int GraceMinutes { get; set; }
    public bool IsNightShift { get; set; }
    public decimal WorkingHours { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// --- HOLIDAYS ---
public class CreateHolidayRequest
{
    public string Name { get; set; } = string.Empty;
    public DateOnly HolidayDate { get; set; }
    public string Type { get; set; } = "Public"; // Public, Regional, Optional
    public int? BranchId { get; set; }
    public bool IsRecurring { get; set; }
    public string? Description { get; set; }
}

public class UpdateHolidayRequest : CreateHolidayRequest { }

public class HolidayDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly HolidayDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public bool IsRecurring { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
