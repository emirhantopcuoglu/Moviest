using Moviest.Services;

namespace Moviest.Tests.Infrastructure;

public class StubEmailSender : IEmailSender
{
    public Task SendAsync(string toAddress, string subject, string htmlBody) => Task.CompletedTask;
}
