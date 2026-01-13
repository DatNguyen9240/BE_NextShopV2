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
            var smtpHost = _configuration["Smtp:Host"] ?? "";
            var smtpPort = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 25;
            var smtpUser = _configuration["Smtp:Username"] ?? string.Empty;
            var smtpPass = _configuration["Smtp:Password"] ?? string.Empty;
            var fromEmail = _configuration["Smtp:FromEmail"] ?? "no-reply@example.com";
            var fromName = _configuration["Smtp:FromName"] ?? "NextShop";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                // Use secure connection when possible
                await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.Auto);

                if (!string.IsNullOrEmpty(smtpUser))
                {
                    await client.AuthenticateAsync(smtpUser, smtpPass);
                }

                await client.SendAsync(message);
                _logger.LogInformation("Email sent to {Email} with subject {Subject}", toEmail, subject);
            }
            catch (System.Exception)
            {
                _logger.LogError("Failed to send email to {Email}", toEmail);
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}