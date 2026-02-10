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
            var fromEmail = Environment.GetEnvironmentVariable("RESEND_FROM_EMAIL") 
                ?? _configuration["Resend:FromEmail"] 
                ?? "onboarding@resend.dev";
            var fromName = Environment.GetEnvironmentVariable("RESEND_FROM_NAME") 
                ?? _configuration["Resend:FromName"] 
                ?? "NextShop";

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
                _logger.LogInformation("Email sent successfully to {Email} with subject {Subject}", toEmail, subject);
            }
            catch (Resend.ResendException ex) when (ex.Message.Contains("only send testing emails"))
            {
                _logger.LogWarning("Cannot send to {Email} - Resend free plan with onboarding@resend.dev only allows sending to account owner email. Please verify a domain at resend.com/domains", toEmail);
                // Don't throw - just log warning in production to avoid blocking user flows
                if (_configuration["ASPNETCORE_ENVIRONMENT"] != "Production")
                {
                    throw;
                }
            }
            catch (Resend.ResendException ex) when (ex.Message.Contains("domain is not verified"))
            {
                _logger.LogWarning("Cannot send from {FromEmail} - Domain not verified. Please verify at resend.com/domains", fromEmail);
                if (_configuration["ASPNETCORE_ENVIRONMENT"] != "Production")
                {
                    throw;
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }
    }
}