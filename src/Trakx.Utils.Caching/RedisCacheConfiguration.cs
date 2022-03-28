using Trakx.Utils.Attributes;

namespace Trakx.Utils.Caching;

public record RedisCacheConfiguration
{
    #nullable disable
    [AwsParameter]
    public string ConnectionString { get; init; }
    #nullable enable
}
