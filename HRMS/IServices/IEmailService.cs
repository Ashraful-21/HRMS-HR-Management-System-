using System.Threading.Tasks;

namespace HRMS.IServices;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlContent, string? plainTextContent = null);
    Task<bool> SendOtpEmailAsync(string toEmail, string toName, string otpCode, int expiryMinutes = 10);
}
