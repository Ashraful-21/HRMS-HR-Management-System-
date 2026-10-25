using HRMS.Models;

namespace HRMS.IServices;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    (string rawToken, string tokenHash, DateTime expiresAt) GenerateRefreshToken(string? ipAddress);
    (string rawToken, string tokenHash, DateTime expiresAt) GeneratePasswordResetToken();
    string HashToken(string token);
    DateTime GetAccessTokenExpiration();
}
