using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SendGrid.Helpers.Mail;
using SendGrid.Helpers.Mail.Model;
using Trakx.Utils.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Trakx.Utils.Emails.Tests.Integration;

public class SendGridEmailSenderTests
{
    private readonly ITestOutputHelper _output;
    private readonly MockCreator _mockCreator;

    public SendGridEmailSenderTests(ITestOutputHelper output)
    {
        _output = output;
        _mockCreator = new MockCreator(output);
    }

    [Fact(Skip = "only worth running if you think this service is broken")]
    public async Task SendAsync_should_send_actual_emails()
    {
        var configuration = ConfigurationHelper.GetConfigurationFromEnv<EmailServiceConfiguration>();
        var services = new ServiceCollection();
        services.AddEmailService(configuration);
        var provider = services.BuildServiceProvider();

        var mailer = provider.GetRequiredService<ISendGridEmailSender>();

        var mailTo = "devs@trakx.io";
        var subject = GetType().FullName + " test";
        var htmlContent = $"<p>this mail was created at <b>{DateTimeOffset.UtcNow}</b></p>";
        var message = _mockCreator.GetRandomSendGridMessage(to: mailTo, subject: subject, htmlContent: htmlContent);

        var result = await mailer.SendAsync(message);
        result.Should().BeTrue();
    }
}
