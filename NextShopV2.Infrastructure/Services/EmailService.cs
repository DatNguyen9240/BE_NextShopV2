using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NextShopV2.Application.Interfaces.Services;
using Resend;

namespace NextShopV2.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IResend _resend;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration, IResend resend)
        {
            _logger = logger;
            _configuration = configuration;
            _resend = resend;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var fromEmail = _configuration["Resend:FromEmail"] ?? "onboarding@resend.dev";
            var fromName = _configuration["Resend:FromName"] ?? "NextShop";

            // Debug log
            _logger.LogInformation("Resend FromEmail: {FromEmail}, FromName: {FromName}", fromEmail, fromName);

            try
            {
                var message = new EmailMessage
                {
                    From = $"{fromName} <{fromEmail}>",
                    To = toEmail,
                    Subject = subject,
                    HtmlBody = htmlBody
                };

                await _resend.EmailSendAsync(message);
                _logger.LogInformation("Email sent to {Email} with subject {Subject}", toEmail, subject);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }
    }
}