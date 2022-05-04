using System.Reflection;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;

namespace Trakx.Utils.Emails;

/// <inheritdoc />
public class SendGridEmailSender : ISendGridEmailSender
{
    private static readonly ILogger Logger = Log.Logger.ForContext(MethodBase.GetCurrentMethod()!.DeclaringType!);
    private readonly ISendGridClient _sendGridClient;

    /// <summary>
    /// Constructor of e-mail sender.
    /// </summary>
    public SendGridEmailSender(ISendGridClient sendGridClient)
    {
        _sendGridClient = sendGridClient;
    }

    /// <inheritdoc />
    public async Task<bool> SendAsync(SendGridMessage msg, CancellationToken cancellationToken = default)
    {
        var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
        {
            Logger.Information("Email '{Subject}' has been sent: http code {StatusCode}",
                msg.Subject, response.StatusCode);
            return true;
        }

        Logger.Error("Unable to deliver e-mail message to '{Tos}': http code {StatusCode}",
            msg.Personalizations.FirstOrDefault()?.Tos, response.StatusCode);
        return false;
    }

}
