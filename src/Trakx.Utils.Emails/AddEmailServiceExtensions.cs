using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid.Extensions.DependencyInjection;
using SendGrid.Helpers.Reliability;

namespace Trakx.Utils.Emails;

public static class AddEmailServiceExtensions
{
    public static void AddEmailService(this IServiceCollection services, IConfiguration configuration)
    {
        var emailServiceConfiguration = configuration.GetSection(nameof(EmailServiceConfiguration))
            .Get<EmailServiceConfiguration>();

        AddEmailService(services, emailServiceConfiguration);
    }

    public static void AddEmailService(this IServiceCollection services, EmailServiceConfiguration emailServiceConfiguration)
    {
        services.AddSingleton(emailServiceConfiguration);
        services.AddSendGrid(options =>
        {
            options.ApiKey = emailServiceConfiguration.SendGridApiKey;
            options.HttpErrorAsException = true;
            options.ReliabilitySettings = new ReliabilitySettings(3,
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(5_000),
                TimeSpan.FromMilliseconds(200));
        });
        services.AddScoped<ISendGridEmailSender, SendGridEmailSender>();
    }
}
