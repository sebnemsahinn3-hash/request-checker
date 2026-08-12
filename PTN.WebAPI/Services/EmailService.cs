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
                var smtpUser = _configuration["Smtp:User"] ?? "sebnemsahinn3@gmail.com";
                var smtpPass = (_configuration["Smtp:Password"] ?? "").Replace(" ", "");

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUser, "PTN Health Monitor Alert"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                // Eğer Gmail uygulama şifresi girilmişse gerçek SMTP iletimi yapılır
                if (!string.IsNullOrEmpty(smtpPass))
                {
                    await client.SendMailAsync(mailMessage);
                    Console.WriteLine($"[GMAIL SMTP GERÇEK E-POSTA İLETİLDİ] Alıcı: {toEmail} | Konu: {subject}");
                }
                else
                {
                    Console.WriteLine($"[GMAIL SMTP UYARISI] Alıcı: {toEmail} | Konu: {subject} (appsettings.json dosyasına Gmail Uygulama Şifresi eklenince E-Posta gerçek mail kutusuna düşer)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GMAIL SMTP HATA BİLDİRİMİ]: {ex.Message}");
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