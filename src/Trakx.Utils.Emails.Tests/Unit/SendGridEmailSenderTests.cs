using System.Net;
using FluentAssertions;
using NSubstitute;
using SendGrid;
using SendGrid.Helpers.Mail;
using Xunit;
using Xunit.Abstractions;

namespace Trakx.Utils.Emails.Tests;

public class SendGridEmailSenderTests
{
    private readonly MockCreator _mockCreator;
    private readonly SendGridEmailSender _sender;
    private readonly ISendGridClient _sendgridClient;

    public SendGridEmailSenderTests(ITestOutputHelper output)
    {
        _mockCreator = new MockCreator(output);
        _sendgridClient = Substitute.For<ISendGridClient>();
        _sender = new SendGridEmailSender(_sendgridClient);
    }

    [Fact]
    public async Task SendAsync_should_send_mail_to_the_destination()
    {
        const string? @from = "from@trakx.io";
        const string? to = "to@trakx.io";
        const string? subject = "my test e-mail";
        const string? content = "<h1>Test e-mail</h1>";
        _sendgridClient.SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>())
            .Returns(new Response(HttpStatusCode.Accepted, null, null));

        var message = _mockCreator.GetRandomSendGridMessage(from, to, subject, content);

        var sent = await _sender.SendAsync(message, CancellationToken.None);

        sent.Should().BeTrue();
        await _sendgridClient.Received(1)
            .SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>());
        var messageSent = (SendGridMessage)_sendgridClient.ReceivedCalls().Single().GetArguments()[0]!;

        messageSent.From.Email.Should().Be(from);
        messageSent.Personalizations.Single().Tos.Single().Email.Should().Be(to);
        messageSent.Personalizations.Single().Subject.Should().Be(subject);
        messageSent.Contents.Should().AllSatisfy(c => c.Value.Should().Be(content));
    }
}
