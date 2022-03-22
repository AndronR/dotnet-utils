using System.IO;
using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Trakx.Utils.Extensions;
using Trakx.Utils.Startup;

namespace Trakx.Utils.Testing
{
    /// <summary>
    /// Use this class to get configuration or configurations stubs populated from various sources.
    /// </summary>
    public static class ConfigurationHelper
    {
        public static void LoadDefaultEnvFile()
        {
            var envFilePath = DirectoryInfoExtensions.GetDefaultEnvFilePath(null);
            if (envFilePath != null) Env.Load(Path.Combine(envFilePath));
        }

        public static T GetConfigurationFromEnv<T>() where T : new()
        {
            LoadDefaultEnvFile();
            var builder = new ConfigurationBuilder().AddEnvironmentVariables();
            var configurationRoot = builder.Build();
            var configuration = configurationRoot.GetSection(typeof(T).Name).Get<T>()!;
            return configuration;
        }

        public static T GetConfigurationFromAws<T>(string? environment = null) where T : new()
        {
            LoadDefaultEnvFile();
            var builder = new ConfigurationBuilder().AddAwsSystemManagerConfiguration<T>(environment);
            var configurationRoot = builder.Build();
            var configuration = configurationRoot.GetSection(typeof(T).Name).Get<T>()!;
            return configuration;
        }
    }
}
