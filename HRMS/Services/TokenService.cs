using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HRMS.Constants;
using HRMS.IServices;
using HRMS.Models;
using HRMS.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HRMS.Services;

public class TokenService(IOptions<JwtSettings> jwtOptions) : ITokenService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(AppClaimTypes.UserId, user.Id.ToString()),
            new(AppClaimTypes.Username, user.Username)
        };

        if (user.EmployeeId.HasValue)
        {
            claims.Add(new Claim(AppClaimTypes.EmployeeId, user.EmployeeId.Value.ToString()));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var perm in permissions.Distinct())
        {
            claims.Add(new Claim(AppClaimTypes.Permission, perm));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = GetAccessTokenExpiration();

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string rawToken, string tokenHash, DateTime expiresAt) GenerateRefreshToken(string? ipAddress)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var rawToken = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        var tokenHash = HashToken(rawToken);
        var expiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays > 0 ? _jwt.RefreshTokenDays : 7);

        return (rawToken, tokenHash, expiresAt);
    }

    public (string rawToken, string tokenHash, DateTime expiresAt) GeneratePasswordResetToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(48);
        var rawToken = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        var tokenHash = HashToken(rawToken);
        var expiresAt = DateTime.UtcNow.AddHours(1); // 1-hour validity

        return (rawToken, tokenHash, expiresAt);
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public DateTime GetAccessTokenExpiration()
    {
        var minutes = _jwt.AccessTokenMinutes > 0 ? _jwt.AccessTokenMinutes : 15;
        return DateTime.UtcNow.AddMinutes(minutes);
    }
}
