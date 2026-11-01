using MailKit.Security;
using MimeKit;
using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Auth.Services.Interfaces;



namespace Auth.Services
{
    public class EmailService:IEmailService
    {


        private readonly IEmailSettingsService _settings;
        public EmailService(IEmailSettingsService settings)
        {
            _settings = settings;
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string htmlMessage)
        {
            // Managed from Email Setup in the Admin Panel (stored encrypted in the database)
            var settings = await _settings.GetForSendingAsync();
            if (recipientEmail == null)
            {
                throw new InvalidOperationException("Recipient email is required.");
            }
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(settings.SenderName, settings.SenderEmail));
            emailMessage.To.Add(new MailboxAddress("", recipientEmail));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    client.Timeout = 20000;
                    // Port 465 is TLS from the first byte; anything else must upgrade with STARTTLS, never plain text
                    var security = settings.SmtpPort == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                    await client.ConnectAsync(settings.SmtpServer, settings.SmtpPort, security);
                    await client.AuthenticateAsync(settings.SenderEmail, settings.AppPassword);
                    await client.SendAsync(emailMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SMTP Error: {ex.Message}");
                    throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }


}
