namespace HRMS.Dtos.Users;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = [];
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public List<string> Roles { get; set; } = [];
}

public class UpdateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
    public List<string> Roles { get; set; } = [];
}

public class ResetUserPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public int UsersCount { get; set; }
    public List<string> Permissions { get; set; } = [];
}

public class UpdateRolePermissionsRequest
{
    public List<string> PermissionCodes { get; set; } = [];
}

public class SystemSettingDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string Category { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
}

public class UpdateSettingRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
