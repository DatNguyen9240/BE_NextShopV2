using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NextShopV2.Application.Interfaces.Services;
using MimeKit;
using MailKit.Net.Smtp;

namespace NextShopV2.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var smtpHost = _configuration["Smtp:Host"] ?? System.Environment.GetEnvironmentVariable("SMTP_HOST") ?? "";
            var smtpPort = int.TryParse(_configuration["Smtp:Port"] ?? System.Environment.GetEnvironmentVariable("SMTP_PORT"), out var p) ? p : 587;
            var smtpUser = _configuration["Smtp:Username"] ?? System.Environment.GetEnvironmentVariable("SMTP_USER") ?? string.Empty;
            var smtpPass = _configuration["Smtp:Password"] ?? System.Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? string.Empty;
            var fromEmail = _configuration["Smtp:FromEmail"] ?? System.Environment.GetEnvironmentVariable("SMTP_USER") ?? "no-reply@nextshop.com";
            var fromName = _configuration["Smtp:FromName"] ?? System.Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? "NextShop";

            // Log config để debug (không log password)
            _logger.LogInformation("📧 SMTP - Host: {Host}, User: {User}, From: {From}", 
                smtpHost, string.IsNullOrEmpty(smtpUser) ? "❌ MISSING" : smtpUser, fromEmail);
            
            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
            {
                _logger.LogError("❌ SMTP credentials not configured! Set SMTP_USER and SMTP_PASSWORD in Railway environment variables.");
                throw new System.InvalidOperationException("SMTP credentials missing");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            // Thử nhiều cách kết nối để tránh bị firewall block
            var connectionAttempts = new[]
            {
                (Port: 587, Option: MailKit.Security.SecureSocketOptions.StartTls, Name: "Port 587 (StartTLS)"),
                (Port: 465, Option: MailKit.Security.SecureSocketOptions.SslOnConnect, Name: "Port 465 (SSL)"),
                (Port: smtpPort, Option: MailKit.Security.SecureSocketOptions.Auto, Name: $"Port {smtpPort} (Auto)")
            };

            Exception? lastException = null;

            foreach (var attempt in connectionAttempts)
            {
                using var client = new SmtpClient();
                try
                {
                    client.Timeout = 15000; // 15s timeout cho mỗi attempt
                    
                    _logger.LogInformation("🔌 Attempting {Method}...", attempt.Name);
                    await client.ConnectAsync(smtpHost, attempt.Port, attempt.Option);

                    if (!string.IsNullOrEmpty(smtpUser))
                    {
                        await client.AuthenticateAsync(smtpUser, smtpPass);
                    }

                    await client.SendAsync(message);
                    _logger.LogInformation("✅ Email sent successfully to {Email} via {Method}", toEmail, attempt.Name);
                    
                    if (client.IsConnected)
                    {
                        await client.DisconnectAsync(true);
                    }
                    return; // Success!
                }
                catch (System.TimeoutException ex)
                {
                    lastException = ex;
                    _logger.LogWarning("⏱️ Timeout on {Method}: {Message}", attempt.Name, ex.Message);
                    if (client.IsConnected)
                    {
                        try { await client.DisconnectAsync(true); } catch { }
                    }
                }
                catch (System.Exception ex)
                {
                    lastException = ex;
                    _logger.LogWarning("⚠️ Failed {Method}: {Message}", attempt.Name, ex.Message);
                    if (client.IsConnected)
                    {
                        try { await client.DisconnectAsync(true); } catch { }
                    }
                }
            }

            // Tất cả attempts đều fail
            _logger.LogError(lastException, 
                "❌ All SMTP attempts failed. Your hosting provider may be blocking SMTP ports. " +
                "Consider using SendGrid/Resend/Mailgun instead. Host: {Host}, User: {User}", 
                smtpHost, smtpUser);
            throw new System.InvalidOperationException(
                "Cannot send email. SMTP ports may be blocked by hosting provider. Use a transactional email service.", 
                lastException);
        }
    }
}