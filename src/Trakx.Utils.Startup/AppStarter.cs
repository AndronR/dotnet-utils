using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Filters;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using DotNetEnv;
using Trakx.Utils.Extensions;

namespace Trakx.Utils.Startup;

/// <summary>
/// Helper class with methods commonly used to configure the startup
/// of applications. Use this to configure logging, application configurations,
/// etc., in a consistent way across projects.
/// </summary>
public static class AppStarter
{
    private const string OutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss}][{Level:u3}]{Properties:lj} {Message} <{SourceContext}>{NewLine}{Exception}";

    /// <summary>
    /// Use this method in the Program.cs to avoid re-writing code that should
    /// stay consistent across projects.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="configureDefaultsAction">Action to be performed </param>
    /// <typeparam name="TStartup"></typeparam>
    /// <returns></returns>
    public static IHost BuildWebHost<TStartup>(string[] args, Action<IWebHostBuilder>? configureDefaultsAction = null) where TStartup : class
    {
        LoadVariablesFromEnvFile();

        var configureBuilderAction = configureDefaultsAction ?? (_ => { });
        var hostBuilder = CreateHostBuilder<TStartup>(args, configureBuilderAction);

        hostBuilder.AddAwsSystemsManagerConfiguration<TStartup>();

        var host = hostBuilder.Build();
        var config = host.Services.GetRequiredService<IConfiguration>();
        var logger = CreateLogger(config);
        var configRoot = host.Services.GetRequiredService<IConfigurationRoot>();
        logger.Information("Host built with configuration: {configRoot}", configRoot.GetDebugView());
        return host;
    }


    /// <summary>
    /// Implement a non generic version of this method in the Program.cs
    /// class of the service using this AppStarter to ensure that Nswag
    /// can still generate API clients on build.
    /// </summary>
    public static IHostBuilder CreateHostBuilder<TStartup>(string[] args, Action<IWebHostBuilder>? configureDefaultsAction = null) where TStartup : class
    {
        var configureBuilderAction = configureDefaultsAction ?? (_ => { });

        var hostBuilder = Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                configureBuilderAction(webBuilder);
                webBuilder.UseStartup<TStartup>();
            })
            .UseSerilog();
        return hostBuilder;
    }

    public static IHost BuildConsoleHost<TProgram>(string[] args, Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        LoadVariablesFromEnvFile();

        var hostBuilder = Host
            .CreateDefaultBuilder(args)
            .ConfigureServices(configureDelegate)
            .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
            .AddAwsSystemsManagerConfiguration<TProgram>()
            .UseSerilog();

        var host = hostBuilder.Build();
        var config = host.Services.GetRequiredService<IConfiguration>();
        var logger = CreateLogger(config);
        var configRoot = host.Services.GetRequiredService<IConfigurationRoot>();
        logger.Information("Host built with configuration: {configRoot}", configRoot.GetDebugView());
        return host;
    }

    /// <summary>
    /// Reads values from AWS parameter store and uses them as a configuration source.
    /// </summary>
    /// <param name="hostBuilder">The host builder to configure with AWS.</param>
    /// <typeparam name="TTypeFromApplicationAssembly">Any type from the assembly giving which name is used to
    /// retrieve variables path on AWS parameter store.</typeparam>
    public static IHostBuilder AddAwsSystemsManagerConfiguration<TTypeFromApplicationAssembly>(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureAppConfiguration(builder => AddAwsSystemManagerConfiguration<TTypeFromApplicationAssembly>(builder));
        return hostBuilder;
    }

    public static IConfigurationBuilder AddAwsSystemManagerConfiguration<TTypeFromApplicationAssembly>(this IConfigurationBuilder builder,
        string? environment = default)
    {
        builder.AddSystemsManager(configSource =>
        {
            var defaultedEnvironment = environment ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
            var application = typeof(TTypeFromApplicationAssembly).Assembly.GetName().Name!;
            configSource.Path = $"/{defaultedEnvironment}/{application.Replace(".", "/")}";
            configSource.ReloadAfter = TimeSpan.FromMinutes(5);
            configSource.Optional = true;
            configSource.OnLoadException += exceptionContext =>
            {
                var logger = CreateDefaultLogger();
                logger.Error(exceptionContext.Exception,
                    "Failed to load config parameters from AWS using path {path}",
                    configSource.Path);
            };
        });
        return builder;
    }

    [Conditional("DEBUG")]
    private static void LoadVariablesFromEnvFile()
    {
        try
        {
            var envFilePath = new DirectoryInfo(Environment.CurrentDirectory).GetDefaultEnvFilePath();
            DotNetEnv.Env.Load(envFilePath);
            Debug.Assert(Environment.GetEnvironmentVariable("IsEnvFileLoaded")!.Equals("Yes"));
        }
        catch { /**/ }
    }

    private static ILogger CreateDefaultLogger()
    {
        var loggerConfiguration = new LoggerConfiguration()
            .WriteTo.Console(
                theme: AnsiConsoleTheme.Code,
                outputTemplate: OutputTemplate
            );
         var defaultLogger = loggerConfiguration
            .CreateLogger()
            .ForContext(MethodBase.GetCurrentMethod()!.DeclaringType);
         return defaultLogger;
    }

    private static ILogger CreateLogger(IConfiguration configuration)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Filter.ByExcluding(Matching.FromSource("Microsoft"))
            .WriteTo.Console(
                theme: AnsiConsoleTheme.Code,
                outputTemplate: OutputTemplate
            );
        Log.Logger = loggerConfiguration
            .CreateLogger()
            .ForContext(MethodBase.GetCurrentMethod()!.DeclaringType);

        return Log.Logger;
    }
}
