using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Moviest.Models;

namespace Moviest.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendAsync(string toAddress, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
            message.To.Add(MailboxAddress.Parse(toAddress));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                var secureSocketOptions = _settings.UseSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

                await client.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions);
                await client.AuthenticateAsync(_settings.UserName, _settings.Password);
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta gönderilemedi: {To}", toAddress);
                throw;
            }
            finally
            {
                await client.DisconnectAsync(quit: true);
            }
        }
    }
}
