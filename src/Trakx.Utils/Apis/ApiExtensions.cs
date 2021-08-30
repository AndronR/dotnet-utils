using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Trakx.Utils.Apis
{
    public static class ApiExtensions
    {
        public static void AddVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(config =>
            {
                var version = Assembly.GetEntryAssembly()?.GetName().Version!;
                config.DefaultApiVersion = new ApiVersion(version.Major, version.Minor);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;

                config.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader("x-api-version"),
                    new MediaTypeApiVersionReader("x-api-version")
                );
            });
        }
    }
}