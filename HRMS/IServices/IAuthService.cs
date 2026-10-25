using HRMS.Dtos.Auth;
using HRMS.Dtos.Common;

namespace HRMS.IServices;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, string? ipAddress);
    Task<ApiResponse<LoginResponse>> VerifyOtpAsync(VerifyOtpRequest request, string? ipAddress);
    Task<ApiResponse<LoginResponse>> ResendOtpAsync(string challengeToken, string? ipAddress);
    Task<ApiResponse<TokenResponse>> RefreshTokenAsync(string refreshToken, string? ipAddress);
    Task<ApiResponse> RevokeTokenAsync(string? refreshToken, int? userId, string? ipAddress);
    Task<ApiResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request);
    Task<ApiResponse<UserInfoDto>> GetCurrentUserAsync(int userId);
}
