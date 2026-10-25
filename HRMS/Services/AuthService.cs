using HRMS.Constants;
using HRMS.Data;
using HRMS.Dtos.Auth;
using HRMS.Dtos.Common;
using HRMS.IServices;
using HRMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Services;

public class AuthService(
    HrmsDbContext db,
    ITokenService tokenService,
    IAuditService auditService,
    ICacheService cache,
    IEmailService emailService,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly PasswordHasher<User> _hasher = new();

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, string? ipAddress)
    {
        var normalized = request.Username.Trim().ToLower();

        var user = await db.Users
            .Include(u => u.Employee)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => !u.IsDeleted &&
                (u.Username.ToLower() == normalized || u.Email.ToLower() == normalized));

        if (user == null)
        {
            return ApiResponse<LoginResponse>.Fail("Invalid credentials.");
        }

        // Account lockout check
        if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > DateTime.UtcNow)
        {
            var remaining = (int)Math.Ceiling((user.LockoutEndUtc.Value - DateTime.UtcNow).TotalMinutes);
            return ApiResponse<LoginResponse>.Fail(
                $"Account is locked due to multiple failed login attempts. Please try again in {remaining} minute(s).");
        }

        if (!user.IsActive)
        {
            return ApiResponse<LoginResponse>.Fail("Account has been deactivated. Please contact your administrator.");
        }

        var verificationResult = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= 5)
            {
                user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(15);
                logger.LogWarning("User {Username} locked out for 15 minutes due to 5 failed attempts.", user.Username);
            }

            await db.SaveChangesAsync();
            return ApiResponse<LoginResponse>.Fail("Invalid credentials.");
        }

        // Password matches! Generate 6-digit OTP and challenge token
        var otpCode = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
        var challengeToken = Guid.NewGuid().ToString("N");

        var otpState = new LoginOtpState
        {
            UserId = user.Id,
            Email = user.Email,
            Username = user.Username,
            OtpCode = otpCode,
            Attempts = 0,
            CreatedAt = DateTime.UtcNow
        };

        await cache.SetAsync($"auth:otp:{challengeToken}", otpState, TimeSpan.FromMinutes(10));

        var recipientName = user.Employee?.FullName ?? user.Username;
        await emailService.SendOtpEmailAsync(user.Email, recipientName, otpCode, 10);

        var maskedEmail = MaskEmail(user.Email);

        return ApiResponse<LoginResponse>.Ok(new LoginResponse
        {
            RequiresTwoFactor = true,
            TwoFactorChallenge = challengeToken,
            MaskedEmail = maskedEmail,
            DevOtpHint = otpCode,
            Message = $"A 6-digit verification code has been dispatched to {maskedEmail}."
        }, "Two-factor verification code dispatched.");
    }

    public async Task<ApiResponse<LoginResponse>> VerifyOtpAsync(VerifyOtpRequest request, string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(request.ChallengeToken) || string.IsNullOrWhiteSpace(request.OtpCode))
        {
            return ApiResponse<LoginResponse>.Fail("Verification challenge and OTP code are required.");
        }

        var cacheKey = $"auth:otp:{request.ChallengeToken.Trim()}";
        var state = await cache.GetAsync<LoginOtpState>(cacheKey);

        if (state == null)
        {
            return ApiResponse<LoginResponse>.Fail("Verification code expired or invalid session. Please sign in again.");
        }

        if (state.OtpCode != request.OtpCode.Trim())
        {
            state.Attempts++;
            if (state.Attempts >= 5)
            {
                await cache.RemoveAsync(cacheKey);
                return ApiResponse<LoginResponse>.Fail("Too many invalid verification attempts. Please sign in again.");
            }

            await cache.SetAsync(cacheKey, state, TimeSpan.FromMinutes(5));
            return ApiResponse<LoginResponse>.Fail($"Invalid verification code. {5 - state.Attempts} attempt(s) remaining.");
        }

        // Valid OTP! Remove challenge token so it cannot be reused
        await cache.RemoveAsync(cacheKey);

        var user = await db.Users
            .Include(u => u.Employee)
                .ThenInclude(e => e!.Department)
            .Include(u => u.Employee)
                .ThenInclude(e => e!.Designation)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == state.UserId && !u.IsDeleted);

        if (user == null || !user.IsActive)
        {
            return ApiResponse<LoginResponse>.Fail("Account is inactive or not found.");
        }

        // Reset lockout and update login time
        user.FailedLoginCount = 0;
        user.LockoutEndUtc = null;
        user.LastLoginAt = DateTime.UtcNow;

        var roles = user.UserRoles
            .Where(ur => !ur.Role.IsDeleted)
            .Select(ur => ur.Role.Name)
            .ToList();

        var permissions = user.UserRoles
            .Where(ur => !ur.Role.IsDeleted)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var accessToken = tokenService.GenerateAccessToken(user, roles, permissions);
        var (rawRefreshToken, tokenHash, refreshExpiresAt) = tokenService.GenerateRefreshToken(ipAddress);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = refreshExpiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            IsRevoked = false
        };

        db.RefreshTokens.Add(refreshTokenEntity);
        await db.SaveChangesAsync();

        await auditService.LogAsync(AuditActions.Login, "Users", user.Id.ToString(), null,
            new { user.Username, ipAddress, Method = "2FA-Email-OTP" });

        var userInfo = BuildUserInfoDto(user, roles, permissions);

        return ApiResponse<LoginResponse>.Ok(new LoginResponse
        {
            RequiresTwoFactor = false,
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            AccessTokenExpiresAt = tokenService.GetAccessTokenExpiration(),
            User = userInfo
        }, "Login verified successfully.");
    }

    public async Task<ApiResponse<LoginResponse>> ResendOtpAsync(string challengeToken, string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(challengeToken))
        {
            return ApiResponse<LoginResponse>.Fail("Invalid challenge session.");
        }

        var cacheKey = $"auth:otp:{challengeToken.Trim()}";
        var state = await cache.GetAsync<LoginOtpState>(cacheKey);

        if (state == null)
        {
            return ApiResponse<LoginResponse>.Fail("Verification session has expired. Please log in again.");
        }

        var newOtpCode = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
        state.OtpCode = newOtpCode;
        state.CreatedAt = DateTime.UtcNow;

        await cache.SetAsync(cacheKey, state, TimeSpan.FromMinutes(10));

        var user = await db.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == state.UserId);
        var recipientName = user?.Employee?.FullName ?? state.Username;
        await emailService.SendOtpEmailAsync(state.Email, recipientName, newOtpCode, 10);

        var maskedEmail = MaskEmail(state.Email);

        return ApiResponse<LoginResponse>.Ok(new LoginResponse
        {
            RequiresTwoFactor = true,
            TwoFactorChallenge = challengeToken,
            MaskedEmail = maskedEmail,
            DevOtpHint = newOtpCode,
            Message = $"A fresh 6-digit verification code has been dispatched to {maskedEmail}."
        }, "New code sent.");
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return email;
        var parts = email.Split('@');
        var name = parts[0];
        var domain = parts[1];
        var maskedName = name.Length <= 2
            ? name.Substring(0, 1) + "***"
            : name.Substring(0, 2) + new string('*', Math.Max(1, name.Length - 3)) + name[^1];
        return $"{maskedName}@{domain}";
    }

    public async Task<ApiResponse<TokenResponse>> RefreshTokenAsync(string refreshToken, string? ipAddress)
    {
        var tokenHash = tokenService.HashToken(refreshToken);

        var storedToken = await db.RefreshTokens
            .Include(r => r.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash);

        if (storedToken == null)
        {
            return ApiResponse<TokenResponse>.Fail("Invalid refresh token.");
        }

        // Reuse detection: If token was already revoked, someone is reusing an old/compromised token!
        if (storedToken.IsRevoked)
        {
            logger.LogWarning("Security Alert: Refresh token reuse detected for User ID {UserId}! Revoking all tokens.", storedToken.UserId);

            var activeTokens = await db.RefreshTokens
                .Where(r => r.UserId == storedToken.UserId && !r.IsRevoked)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = ipAddress;
            }

            await db.SaveChangesAsync();
            return ApiResponse<TokenResponse>.Fail("Compromised session detected. All refresh tokens revoked. Please log in again.");
        }

        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedByIp = ipAddress;
            await db.SaveChangesAsync();
            return ApiResponse<TokenResponse>.Fail("Refresh token has expired. Please log in again.");
        }

        var user = storedToken.User;
        if (user.IsDeleted || !user.IsActive)
        {
            return ApiResponse<TokenResponse>.Fail("User account is inactive or disabled.");
        }

        // Rotate token
        var (newRawRefresh, newHash, newExpiresAt) = tokenService.GenerateRefreshToken(ipAddress);

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = ipAddress;
        storedToken.ReplacedByTokenHash = newHash;

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newHash,
            ExpiresAt = newExpiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            IsRevoked = false
        };

        db.RefreshTokens.Add(newRefreshToken);

        var roles = user.UserRoles
            .Where(ur => !ur.Role.IsDeleted)
            .Select(ur => ur.Role.Name)
            .ToList();

        var permissions = user.UserRoles
            .Where(ur => !ur.Role.IsDeleted)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var newAccessToken = tokenService.GenerateAccessToken(user, roles, permissions);
        await db.SaveChangesAsync();

        return ApiResponse<TokenResponse>.Ok(new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRawRefresh,
            AccessTokenExpiresAt = tokenService.GetAccessTokenExpiration()
        }, "Token refreshed successfully.");
    }

    public async Task<ApiResponse> RevokeTokenAsync(string? refreshToken, int? userId, string? ipAddress)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var hash = tokenService.HashToken(refreshToken);
            var token = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);
            if (token != null && !token.IsRevoked)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = ipAddress;
            }
        }
        else if (userId.HasValue)
        {
            var tokens = await db.RefreshTokens
                .Where(r => r.UserId == userId.Value && !r.IsRevoked)
                .ToListAsync();

            foreach (var t in tokens)
            {
                t.IsRevoked = true;
                t.RevokedAt = DateTime.UtcNow;
                t.RevokedByIp = ipAddress;
            }
        }

        await db.SaveChangesAsync();

        if (userId.HasValue)
        {
            await auditService.LogAsync(AuditActions.Logout, "Users", userId.Value.ToString(), null, null);
        }

        return ApiResponse.OkEmpty("Logged out successfully.");
    }

    public async Task<ApiResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        if (user == null)
            return ApiResponse.Fail("User not found.");

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (verify == PasswordVerificationResult.Failed)
            return ApiResponse.Fail("Current password is incorrect.");

        user.PasswordHash = _hasher.HashPassword(user, request.NewPassword);
        user.MustChangePassword = false;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = userId;

        // Revoke all existing refresh tokens
        var tokens = await db.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ToListAsync();

        foreach (var t in tokens)
        {
            t.IsRevoked = true;
            t.RevokedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Users", userId.ToString(), null, new { Action = "PasswordChanged" });

        return ApiResponse.OkEmpty("Password changed successfully. Please log in with your new password.");
    }

    public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLower();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && !u.IsDeleted && u.IsActive);

        if (user != null)
        {
            var (rawToken, tokenHash, expiresAt) = tokenService.GeneratePasswordResetToken();

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            };

            db.PasswordResetTokens.Add(resetToken);
            await db.SaveChangesAsync();

            // In production, an email would be dispatched via SMTP/EmailService.
            // For development, we log it.
            logger.LogInformation("Password reset token generated for {Email}: {Token} (Expires {ExpiresAt})",
                user.Email, rawToken, expiresAt);
        }

        // Return same message regardless to prevent user enumeration
        return ApiResponse.OkEmpty("If an account with that email exists, password reset instructions have been sent.");
    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var tokenHash = tokenService.HashToken(request.Token);

        var resetToken = await db.PasswordResetTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash && r.UsedAt == null && r.ExpiresAt > DateTime.UtcNow);

        if (resetToken == null)
            return ApiResponse.Fail("Invalid or expired password reset token.");

        var user = resetToken.User;
        if (user.IsDeleted || !user.IsActive || !string.Equals(user.Email, request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
            return ApiResponse.Fail("Invalid password reset request.");

        user.PasswordHash = _hasher.HashPassword(user, request.NewPassword);
        user.MustChangePassword = false;
        user.UpdatedAt = DateTime.UtcNow;

        resetToken.UsedAt = DateTime.UtcNow;

        // Invalidate active refresh tokens
        var tokens = await db.RefreshTokens.Where(r => r.UserId == user.Id && !r.IsRevoked).ToListAsync();
        foreach (var t in tokens)
        {
            t.IsRevoked = true;
            t.RevokedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        await auditService.LogAsync(AuditActions.Update, "Users", user.Id.ToString(), null, new { Action = "PasswordReset" });

        return ApiResponse.OkEmpty("Password reset successful. You may now log in.");
    }

    public async Task<ApiResponse<UserInfoDto>> GetCurrentUserAsync(int userId)
    {
        var user = await db.Users
            .Include(u => u.Employee!)
                .ThenInclude(e => e.Department)
            .Include(u => u.Employee!)
                .ThenInclude(e => e.Designation)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
            return ApiResponse<UserInfoDto>.Fail("User not found.");

        var roles = user.UserRoles
            .Where(ur => !ur.Role.IsDeleted)
            .Select(ur => ur.Role.Name)
            .ToList();

        var permissions = user.UserRoles
            .Where(ur => !ur.Role.IsDeleted)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var userInfo = BuildUserInfoDto(user, roles, permissions);
        return ApiResponse<UserInfoDto>.Ok(userInfo);
    }

    private static UserInfoDto BuildUserInfoDto(User user, List<string> roles, List<string> permissions)
    {
        return new UserInfoDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            EmployeeId = user.EmployeeId,
            FullName = user.Employee?.FullName,
            EmployeeCode = user.Employee?.EmployeeCode,
            Designation = user.Employee?.Designation?.Title,
            Department = user.Employee?.Department?.Name,
            ProfilePhotoUrl = user.Employee?.ProfilePhotoUrl,
            MustChangePassword = user.MustChangePassword,
            Roles = roles,
            Permissions = permissions
        };
    }
}
