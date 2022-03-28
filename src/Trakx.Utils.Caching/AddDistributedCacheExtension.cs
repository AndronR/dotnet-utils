using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Trakx.Utils.Caching;

public static class AddDistributedCacheExtension
{
    public static void AddDistributedCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfig = configuration
            .GetSection(nameof(RedisCacheConfiguration)).Get<RedisCacheConfiguration>();
        services.AddSingleton(redisConfig);
        services.AddDistributedRedisCache(options =>
        {
            options.Configuration = redisConfig.ConnectionString;
        });
    }
}