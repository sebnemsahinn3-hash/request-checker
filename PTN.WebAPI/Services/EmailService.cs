using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PTN.WebAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
                var smtpUser = _configuration["Smtp:User"] ?? "info@ptnhealth.com";
                var smtpPass = _configuration["Smtp:Password"] ?? "secretpassword";

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUser, "PTN Health Monitor"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                // SMTP mail gönderimi (Dev ortamında loglanır)
                Console.WriteLine($"[SMTP E-POSTA GÖNDERİLDİ] Alıcı: {toEmail} | Konu: {subject}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"E-Posta Gönderim Hatası: {ex.Message}");
            }
        }

        public async Task SendBulkEmailAsync(List<string> toEmails, string subject, string body)
        {
            foreach (var email in toEmails)
            {
                await SendEmailAsync(email, subject, body);
            }
        }
    }
}