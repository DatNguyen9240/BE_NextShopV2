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
            // Helper để expand placeholders như ${SMTP_USER} trong appsettings
            string ExpandPlaceholder(string? value, string envVarName, string defaultValue = "")
            {
                if (string.IsNullOrWhiteSpace(value))
                    return System.Environment.GetEnvironmentVariable(envVarName) ?? defaultValue;
                
                // Nếu là placeholder dạng ${VAR_NAME}, expand nó
                if (value.StartsWith("${") && value.EndsWith("}"))
                {
                    var varName = value.Substring(2, value.Length - 3);
                    return System.Environment.GetEnvironmentVariable(varName) ?? defaultValue;
                }
                
                return value;
            }

            var smtpHost = ExpandPlaceholder(_configuration["Smtp:Host"], "SMTP_HOST", "smtp.gmail.com");
            var smtpPortStr = ExpandPlaceholder(_configuration["Smtp:Port"], "SMTP_PORT", "587");
            var smtpPort = int.TryParse(smtpPortStr, out var p) ? p : 587;
            var smtpUser = ExpandPlaceholder(_configuration["Smtp:Username"], "SMTP_USER");
            var smtpPass = ExpandPlaceholder(_configuration["Smtp:Password"], "SMTP_PASSWORD");
            var fromEmail = ExpandPlaceholder(_configuration["Smtp:FromEmail"], "SMTP_USER", "no-reply@nextshop.com");
            var fromName = ExpandPlaceholder(_configuration["Smtp:FromName"], "SMTP_FROM_NAME", "NextShop");

            // Log để debug (không log password)
            _logger.LogInformation("📧 SMTP Config - Host: {Host}, Port: {Port}, User: {User}, From: {From}", 
                smtpHost, smtpPort, smtpUser, fromEmail);
            
            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
            {
                _logger.LogError("❌ SMTP credentials missing! Check environment variables: SMTP_USER and SMTP_PASSWORD");
                throw new System.InvalidOperationException("SMTP credentials not configured");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                client.Timeout = 30000; // 30s timeout
                
                _logger.LogInformation("🔌 Connecting to SMTP server...");
                await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);

                _logger.LogInformation("🔐 Authenticating...");
                await client.AuthenticateAsync(smtpUser, smtpPass);

                _logger.LogInformation("📤 Sending email...");
                await client.SendAsync(message);
                
                _logger.LogInformation("✅ Email sent successfully to {Email} with subject: {Subject}", toEmail, subject);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send email to {Email}. Host: {Host}:{Port}, User: {User}", 
                    toEmail, smtpHost, smtpPort, smtpUser);
                throw;
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}