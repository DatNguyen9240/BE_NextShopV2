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

            // Retry với multiple ports để tránh firewall/network issues
            var attempts = new[]
            {
                (Port: smtpPort, Options: MailKit.Security.SecureSocketOptions.StartTls, Name: "StartTls"),
                (Port: 465, Options: MailKit.Security.SecureSocketOptions.SslOnConnect, Name: "SSL"),
                (Port: 587, Options: MailKit.Security.SecureSocketOptions.Auto, Name: "Auto")
            };

            Exception? lastException = null;

            foreach (var attempt in attempts)
            {
                using var client = new SmtpClient();
                try
                {
                    // Timeout 15s cho mỗi attempt
                    client.Timeout = 15000;
                    
                    _logger.LogInformation("Attempting to connect via {Method} on port {Port} to {Host}", 
                        attempt.Name, attempt.Port, smtpHost);
                    
                    await client.ConnectAsync(smtpHost, attempt.Port, attempt.Options);

                    if (!string.IsNullOrEmpty(smtpUser))
                    {
                        await client.AuthenticateAsync(smtpUser, smtpPass);
                    }

                    await client.SendAsync(message);
                    _logger.LogInformation("Email sent successfully to {Email} via {Method}:{Port}", 
                        toEmail, attempt.Name, attempt.Port);
                    
                    if (client.IsConnected)
                    {
                        await client.DisconnectAsync(true);
                    }
                    return; // Success!
                }
                catch (System.TimeoutException tex)
                {
                    lastException = tex;
                    _logger.LogWarning("Timeout connecting to {Host}:{Port} via {Method}: {Message}", 
                        smtpHost, attempt.Port, attempt.Name, tex.Message);
                    
                    if (client.IsConnected)
                    {
                        try { await client.DisconnectAsync(true); } catch { }
                    }
                    // Try next method
                }
                catch (System.Exception ex)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "Failed to send via {Method}:{Port}, trying next method", 
                        attempt.Name, attempt.Port);
                    
                    if (client.IsConnected)
                    {
                        try { await client.DisconnectAsync(true); } catch { }
                    }
                    // Try next method
                }
            }

            // All attempts failed
            _logger.LogError(lastException, "All SMTP connection attempts failed for {Email}. Host: {Host}, User: {User}", 
                toEmail, smtpHost, smtpUser);
            throw new System.InvalidOperationException(
                $"Cannot connect to SMTP server {smtpHost}. This may be due to firewall or network restrictions on your hosting provider. " +
                "Consider using a transactional email service like SendGrid, AWS SES, or Mailgun for production.", 
                lastException);
        }
    }
}