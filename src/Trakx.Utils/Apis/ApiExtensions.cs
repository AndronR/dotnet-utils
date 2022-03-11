using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;

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
}