namespace Auth.Services.Interfaces
{
    public interface IEmailService
    {
        public Task SendEmailAsync(string recipientEmail, string subject, string htmlMessage);
    }
}
