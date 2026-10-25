namespace HRMS.Dtos.Auth;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class VerifyOtpRequest
{
    public string ChallengeToken { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}

public class ResendOtpRequest
{
    public string ChallengeToken { get; set; } = string.Empty;
}

public class LoginOtpState
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class LoginResponse
{
    public bool RequiresTwoFactor { get; set; }
    public string? TwoFactorChallenge { get; set; }
    public string? MaskedEmail { get; set; }
    public string? DevOtpHint { get; set; }
    public string? Message { get; set; }

    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public UserInfoDto? User { get; set; }
}

public class UserInfoDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public string? FullName { get; set; }
    public string? EmployeeCode { get; set; }
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool MustChangePassword { get; set; }
    public List<string> Roles { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class RevokeTokenRequest
{
    public string? RefreshToken { get; set; }
}
