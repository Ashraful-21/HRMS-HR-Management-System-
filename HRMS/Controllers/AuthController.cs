using HRMS.Dtos.Auth;
using HRMS.Dtos.Common;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController(
    IAuthService authService,
    ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>
    /// Authenticate with username/email and password.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ipAddress = GetClientIpAddress();
        var result = await authService.LoginAsync(request, ipAddress);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Verify 6-digit email OTP for two-factor login.
    /// </summary>
    [HttpPost("verify-otp")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var ipAddress = GetClientIpAddress();
        var result = await authService.VerifyOtpAsync(request, ipAddress);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Resend fresh 6-digit email OTP.
    /// </summary>
    [HttpPost("resend-otp")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
    {
        var ipAddress = GetClientIpAddress();
        var result = await authService.ResendOtpAsync(request.ChallengeToken, ipAddress);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Refresh an expired access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = GetClientIpAddress();
        var result = await authService.RefreshTokenAsync(request.RefreshToken, ipAddress);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>
    /// Revoke current refresh token and log out.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RevokeTokenRequest? request = null)
    {
        var ipAddress = GetClientIpAddress();
        var result = await authService.RevokeTokenAsync(request?.RefreshToken, currentUser.UserId, ipAddress);
        return Ok(result);
    }

    /// <summary>
    /// Change the authenticated user's password.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!currentUser.UserId.HasValue)
            return Unauthorized(ApiResponse.Fail("User is not authenticated."));

        var result = await authService.ChangePasswordAsync(currentUser.UserId.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Request a password reset link/token via email.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await authService.ForgotPasswordAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Reset password using a valid reset token.
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await authService.ResetPasswordAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get the current authenticated user's profile, roles, and permissions.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserInfoDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        if (!currentUser.UserId.HasValue)
            return Unauthorized(ApiResponse<UserInfoDto>.Fail("User is not authenticated."));

        var result = await authService.GetCurrentUserAsync(currentUser.UserId.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    private string? GetClientIpAddress()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
            return forwarded.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}
