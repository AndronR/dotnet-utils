using Microsoft.Extensions.Logging;
using Polly;

namespace Trakx.Utils.Extensions;

public static class PollyContextExtensions
{
    public static bool TryGetLogger(this Context context, out ILogger? logger)
    {
        if (context.TryGetValue("Logger", out var loggerObject) && loggerObject is ILogger contextLogger)
        {
            logger = contextLogger;
            return true;
        }

        logger = null;
        return false;
    }
}