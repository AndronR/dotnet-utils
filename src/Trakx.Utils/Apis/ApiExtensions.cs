using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Trakx.Utils.Apis;

public static class ApiExtensions
{
    public static void AddVersioning(this IServiceCollection services, string version)
    {
        var apiVersion = ApiVersion.Parse(version);
        AddVersioning(services, apiVersion);
    }

    public static void AddVersioning(this IServiceCollection services, ApiVersion apiVersion)
    {
        services.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = apiVersion;
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;

            config.ApiVersionReader = ApiVersionReader.Combine(
                new HeaderApiVersionReader("x-api-version")
            );
        });
    }

    public static void LogApiFailure(this Serilog.ILogger logger, DelegateResult<HttpResponseMessage> result, TimeSpan timeSpan, int retryCount, Context context)
    {
         if (result.Exception != null)
         {
             logger.Warning(result.Exception, "An exception occurred on retry {RetryAttempt} for {PolicyKey} - Retrying in {SleepDuration}ms",
                 retryCount, context.PolicyKey, timeSpan.TotalMilliseconds);
         }
         else
         {
             logger.Warning("A non success code {StatusCode} with reason {Reason} and content {Content} was received on retry {RetryAttempt} for {PolicyKey} - Retrying in {SleepDuration}ms",
                 (int)result.Result.StatusCode, result.Result.ReasonPhrase,
                 result.Result.Content?.ReadAsStringAsync().GetAwaiter().GetResult(),
                 retryCount, context.PolicyKey, timeSpan.TotalMilliseconds);
         }
    }
}
