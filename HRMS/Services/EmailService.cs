using System;
using System.Threading.Tasks;
using HRMS.IServices;
using HRMS.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HRMS.Services;

public class EmailService : IEmailService
{
    private readonly SmtpOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpOptions> options, ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlContent, string? plainTextContent = null)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlContent,
                TextBody = plainTextContent ?? htmlContent
            };
            message.Body = bodyBuilder.ToMessageBody();

            // Check if SMTP is configured for real delivery
            bool isLocalDev = string.IsNullOrWhiteSpace(_options.Host) ||
                              _options.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(_options.Username);

            if (isLocalDev)
            {
                _logger.LogInformation("[LOCAL EMAIL DISPATCH] To: {Email} | Subject: {Subject}", toEmail, subject);
                return true;
            }

            using var client = new SmtpClient();
            client.Timeout = 10000; // 10s timeout
            
            var secureOption = _options.EnableSsl ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.Auto;
            await client.ConnectAsync(_options.Host, _options.Port, secureOption);

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                await client.AuthenticateAsync(_options.Username, _options.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email successfully dispatched to {Email} via SMTP ({Host})", toEmail, _options.Host);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deliver email to {Email} via SMTP ({Host}). Falling back to development log.", toEmail, _options.Host);
            return false;
        }
    }

    public async Task<bool> SendOtpEmailAsync(string toEmail, string recipientName, string otpCode, int expiryMinutes = 10)
    {
        var subject = $"[{otpCode}] Your HRMS Portal Verification Code";
        
        var html = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <style>
    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #0b0f19; color: #f1f5f9; padding: 20px; }}
    .card {{ max-width: 520px; margin: 0 auto; background: #131b2e; border: 1px solid #1e293b; border-radius: 12px; padding: 32px; box-shadow: 0 10px 25px rgba(0,0,0,0.5); }}
    .header {{ text-align: center; margin-bottom: 24px; }}
    .logo-badge {{ display: inline-block; padding: 6px 14px; background: rgba(99, 102, 241, 0.15); border: 1px solid #6366f1; border-radius: 9999px; color: #818cf8; font-size: 13px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.05em; }}
    h1 {{ color: #ffffff; font-size: 22px; margin: 16px 0 8px; font-weight: 800; }}
    p {{ color: #94a3b8; font-size: 14px; line-height: 1.6; }}
    .otp-box {{ background: #0b0f19; border: 2px dashed #6366f1; border-radius: 10px; padding: 20px; text-align: center; margin: 28px 0; }}
    .otp-code {{ font-family: 'SF Mono', Consolas, Monaco, monospace; font-size: 38px; font-weight: 800; letter-spacing: 12px; color: #38bdf8; }}
    .meta {{ font-size: 12px; color: #64748b; margin-top: 8px; }}
    .footer {{ border-top: 1px solid #1e293b; margin-top: 28px; padding-top: 16px; font-size: 12px; color: #64748b; text-align: center; }}
  </style>
</head>
<body>
  <div class='card'>
    <div class='header'>
      <div class='logo-badge'>HRMS Enterprise Workspace</div>
      <h1>Two-Factor Verification Code</h1>
      <p>Hello <strong>{recipientName}</strong>,<br>A login attempt requires two-factor identity verification. Enter the single-use passcode below to enter your workspace.</p>
    </div>

    <div class='otp-box'>
      <div class='otp-code'>{otpCode}</div>
      <div class='meta'>Valid for <strong>{expiryMinutes} minutes</strong> • Single use only</div>
    </div>

    <p style='font-size: 13px;'>If you did not initiate this login request, your credentials might be compromised. Please notify your IT System Administrator immediately and reset your password.</p>

    <div class='footer'>
      &copy; {DateTime.UtcNow.Year} HRMS Enterprise Portal • Automated Security Delivery
    </div>
  </div>
</body>
</html>";

        var text = $"Hello {recipientName},\nYour HRMS verification code is: {otpCode}\nThis code will expire in {expiryMinutes} minutes.\nIf you did not request this code, please contact your administrator.";

        _logger.LogInformation("==========================================================");
        _logger.LogInformation(">> [HRMS 2FA SECURITY] OTP for {Email} ({Name}): {OtpCode} <<", toEmail, recipientName, otpCode);
        _logger.LogInformation("==========================================================");

        return await SendEmailAsync(toEmail, recipientName, subject, html, text);
    }
}
