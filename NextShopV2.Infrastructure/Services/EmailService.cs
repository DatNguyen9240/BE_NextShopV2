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
            // Helper để expand environment variable placeholders như ${SMTP_USER}
            string ExpandEnvVar(string? value, string envVarName, string defaultValue = "")
            {
                if (string.IsNullOrWhiteSpace(value))
                    return System.Environment.GetEnvironmentVariable(envVarName) ?? defaultValue;
                
                // Nếu là placeholder như ${SMTP_USER}, expand nó
                if (value.StartsWith("${") && value.EndsWith("}"))
                {
                    var varName = value.Substring(2, value.Length - 3);
                    return System.Environment.GetEnvironmentVariable(varName) ?? defaultValue;
                }
                
                return value;
            }

            var smtpHost = ExpandEnvVar(_configuration["Smtp:Host"], "SMTP_HOST", "smtp.gmail.com");
            var smtpPortStr = ExpandEnvVar(_configuration["Smtp:Port"], "SMTP_PORT", "587");
            var smtpPort = int.TryParse(smtpPortStr, out var p) ? p : 587;
            var smtpUser = ExpandEnvVar(_configuration["Smtp:Username"], "SMTP_USER");
            var smtpPass = ExpandEnvVar(_configuration["Smtp:Password"], "SMTP_PASSWORD");
            var fromEmail = ExpandEnvVar(_configuration["Smtp:FromEmail"], "SMTP_USER", "no-reply@nextshop.com");
            var fromName = ExpandEnvVar(_configuration["Smtp:FromName"], "SMTP_FROM_NAME", "NextShop");

            // Log config (không log password)
            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
            {
                _logger.LogWarning("SMTP credentials missing! User: {HasUser}, Pass: {HasPass}", 
                    !string.IsNullOrEmpty(smtpUser), !string.IsNullOrEmpty(smtpPass));
            }
            else
            {
                _logger.LogInformation("SMTP configured: Host={Host}, Port={Port}, User={User}, From={From}", 
                    smtpHost, smtpPort, smtpUser, fromEmail);
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
                // Set timeout để tránh connection hang
                client.Timeout = 30000; // 30 seconds
                
                _logger.LogInformation("Attempting to send email to {Email} via {Host}:{Port}", toEmail, smtpHost, smtpPort);
                
                // Use StartTls cho Gmail (ổn định hơn Auto)
                await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);

                if (!string.IsNullOrEmpty(smtpUser))
                {
                    await client.AuthenticateAsync(smtpUser, smtpPass);
                }

                await client.SendAsync(message);
                _logger.LogInformation("Email sent successfully to {Email} with subject {Subject}", toEmail, subject);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}. Host: {Host}, Port: {Port}, User: {User}", 
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