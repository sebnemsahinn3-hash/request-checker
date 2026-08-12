using System.Collections.Generic;
using System.Threading.Tasks;

namespace PTN.WebAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendBulkEmailAsync(List<string> toEmails, string subject, string body);
    }
}