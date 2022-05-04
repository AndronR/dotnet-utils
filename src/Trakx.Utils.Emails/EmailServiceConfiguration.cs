using Trakx.Utils.Attributes;

namespace Trakx.Utils.Emails;

public record EmailServiceConfiguration
{
    /// <summary>
    /// Api key to send e-mails via sendgrid. This key is used by <see cref="SendGridEmailSender"/>.
    /// </summary>
    [AwsParameter, SecretEnvironmentVariable]
    public string? SendGridApiKey { get; init; }
}
