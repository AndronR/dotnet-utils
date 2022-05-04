using SendGrid.Helpers.Mail;

namespace Trakx.Utils.Emails;

/// <summary>
/// Mailing services relying on Sendgrid Api.
/// </summary>
public interface ISendGridEmailSender
{
    /// <summary>
    /// Use this method to send an email.
    /// </summary>
    Task<bool> SendAsync(SendGridMessage msg, CancellationToken cancellationToken = default);
}
