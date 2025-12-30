using Microsoft.Extensions.Options;
using RestoBook_MiniProjet.Models;
using System.Net;
using System.Net.Mail;

namespace RestoBook_MiniProjet.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public void Send(string toEmail, string subject, string body)
        {
            try
            {
                var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
                {
                    Credentials = new NetworkCredential(
                        _settings.Username,
                        _settings.Password
                    ),
                    EnableSsl = true
                };

                var message = new MailMessage
                {
                    From = new MailAddress(
                        _settings.SenderEmail,
                        _settings.SenderName
                    ),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);
                client.Send(message);
            }
            catch (Exception ex)
            {
                // l'email ne doit PAS casser l'application
                Console.WriteLine($"[Email Service Error] Failed to send email to {toEmail}: {ex.Message}");
            }
        }
    }
}
