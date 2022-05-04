using SendGrid.Helpers.Mail;
using Xunit.Abstractions;

namespace Trakx.Utils.Emails.Tests;

public class MockCreator : Utils.Testing.MockCreator
{
    public MockCreator(ITestOutputHelper output) : base(output)
    {
    }

    public SendGridMessage GetRandomSendGridMessage(string? from = default,
        string? to = default, string? subject = default, string? htmlContent = default)
    {
        var msg = new SendGridMessage
        {
            From = new EmailAddress(from ?? GetEmailAddress("trakx.io")),
        };
        msg.AddTo(to ?? GetEmailAddress());
        msg.SetSubject(subject ?? GetRandomString(30));
        msg.AddContent("html/text", htmlContent ?? $"<h1>{GetRandomString(30)}</h1>");

        return msg;
    }
}
