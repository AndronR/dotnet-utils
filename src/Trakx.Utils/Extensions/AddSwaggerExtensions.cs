using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Trakx.Utils.Extensions;

public static class AddSwaggerExtensions
{
    /// <summary>
    /// Registers dependencies needed to provide a Swagger documentation to the API.
    /// </summary>
    /// <param name="services">THe collection on service collection on which registration should be
    ///     performed</param>
    /// <param name="apiName">Name for the API</param>
    /// <param name="apiVersion">SemVer version of the API, usually starts with 'v' like v0.1</param>
    /// <param name="apiDescription">Short description of the purpose of the documented API</param>
    /// <param name="assemblyName">use Assembly.GetExecutingAssembly().GetName().Name</param>
    /// <param name="authDomain"></param>
    public static void AddSwaggerJsonAndUi(this IServiceCollection services, string apiName, string apiVersion,
        string apiDescription, string? assemblyName = null, string? authDomain = null)
    {
        services.AddSwaggerJsonAndUi(apiName, apiVersion, apiDescription, (options) =>
        {
            if (authDomain == default) return;

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    ClientCredentials = new OpenApiOAuthFlow
                    {
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "openid" },
                            { "profile", "profile" },
                            { "email", "email" },
                            { "permissions", "permissions" },
                            { "scope", "scope" },
                        },

                        AuthorizationUrl = new Uri($"https://{authDomain}/authorize")
                    }
                }
            });
        }, assemblyName);
    }

    /// <summary>
    /// Registers dependencies needed to provide a Swagger documentation to a API which accepts
    /// bearer authentication.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="apiName">Name of the API.</param>
    /// <param name="apiVersion">The API version.</param>
    /// <param name="apiDescription">The API description.</param>
    /// <param name="assemblyName">Name of the assembly.</param>
    public static void AddSwaggerJsonAndUiBearerApi(this IServiceCollection services, string apiName, string apiVersion,
        string apiDescription, string? assemblyName = null)
    {
        services.AddSwaggerJsonAndUi(apiName, apiVersion, apiDescription, (options) =>
        {
            var securitySchema = new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };
            options.AddSecurityDefinition("Bearer", securitySchema);

            var securityRequirement = new OpenApiSecurityRequirement();
            securityRequirement.Add(securitySchema, new[] { "Bearer" });
            options.AddSecurityRequirement(securityRequirement);
        }, assemblyName);
    }


    /// <summary>
    /// Registers dependencies needed to provide a Swagger documentation to the API.
    /// </summary>
    /// <param name="services">THe collection on service collection on which registration should be
    ///     performed</param>
    /// <param name="apiName">Name for the API</param>
    /// <param name="apiVersion">SemVer version of the API, usually starts with 'v' like v0.1</param>
    /// <param name="apiDescription">Short description of the purpose of the documented API</param>
    /// <param name="assemblyName">use Assembly.GetExecutingAssembly().GetName().Name</param>
    /// <param name="configureSwaggerGen">Configuration options for swagger gen</param>
    private static void AddSwaggerJsonAndUi(this IServiceCollection services, string apiName, string apiVersion,
        string apiDescription, Action<SwaggerGenOptions> configureSwaggerGen, string? assemblyName = null)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(apiVersion, new OpenApiInfo { Title = apiName, Version = apiVersion });
            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{assemblyName ?? Assembly.GetEntryAssembly()?.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
            configureSwaggerGen(c);
        });

        services.AddOpenApiDocument(settings =>
        {
            settings.PostProcess = document =>
            {
                document.Info.Version = apiVersion;
                document.Info.Title = apiName;
                document.Info.Description = apiDescription;
            };
        });
    }

    /// <summary>
    /// Sets up a web UI page to interact with the server via http requests.
    /// </summary>
    /// <param name="app">The web application builder</param>
    /// <param name="apiName">Name for the API</param>
    /// <param name="apiVersion">SemVer version of the API, usually starts with 'v' like v0.1</param>
    /// <param name="setSwaggerUiAsDefaultPage">Use the swaggerUI as default server page</param>
    public static void UseSwaggerUiWithForwardedHost(this IApplicationBuilder app,
        string apiName,
        string apiVersion,
        bool setSwaggerUiAsDefaultPage)
    {
        app.UseSwagger(options =>
        {
            options.PreSerializeFilters.Add((swaggerDoc, httpRequest) =>
            {
                if (!httpRequest.Headers.ContainsKey("X-Forwarded-Host")) return;

                var serverUrl = $"{httpRequest.Headers["X-Forwarded-Proto"]}://" +
                                $"{httpRequest.Headers["X-Forwarded-Host"]}/" +
                                $"{httpRequest.Headers["X-Forwarded-Prefix"]}";

                swaggerDoc.Servers = new List<OpenApiServer> { new() { Url = serverUrl } };
            });
        });
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", apiName);
            c.InjectJavascript("public/index.js");
        });

        if (setSwaggerUiAsDefaultPage)
            app.UseRewriter(new RewriteOptions().AddRedirect("$^", "swagger"));
    }
}