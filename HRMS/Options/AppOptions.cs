namespace HRMS.Options;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey          { get; init; } = string.Empty;
    public string Issuer             { get; init; } = string.Empty;
    public string Audience           { get; init; } = string.Empty;
    public int    AccessTokenMinutes  { get; init; } = 15;
    public int    RefreshTokenDays    { get; init; } = 7;
}

public class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminEmail    { get; init; } = "admin@hrms.local";
    public string AdminPassword { get; init; } = "Admin@123456";
    public string AdminUsername { get; init; } = "superadmin";
    public bool   Demo          { get; init; } = false;
}

public class CloudinaryOptions
{
    public const string SectionName = "Cloudinary";

    public string CloudName { get; init; } = string.Empty;
    public string ApiKey    { get; init; } = string.Empty;
    public string ApiSecret { get; init; } = string.Empty;
    public bool   UseLocal  { get; init; } = true;   // true → LocalFileStorageService
}

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host        { get; init; } = "localhost";
    public int    Port        { get; init; } = 587;
    public string Username    { get; init; } = string.Empty;
    public string Password    { get; init; } = string.Empty;
    public string FromEmail   { get; init; } = "noreply@hrms.local";
    public string FromName    { get; init; } = "HRMS System";
    public bool   EnableSsl   { get; init; } = true;
}

public class CorsOptions
{
    public const string SectionName = "Cors";
    public string[] AllowedOrigins { get; init; } = [];
}

public class RedisOptions
{
    public const string SectionName = "Redis";
    public bool Enabled { get; init; } = false;
    public string ConnectionString { get; init; } = "localhost:6379,abortConnect=false";
    public string InstanceName { get; init; } = "HRMS:";
}

