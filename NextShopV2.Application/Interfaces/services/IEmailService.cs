using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Send an email with HTML body.
        /// </summary>
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}