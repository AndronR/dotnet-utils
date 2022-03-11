using Microsoft.AspNetCore.Mvc;

namespace Trakx.Utils.Extensions;

public static class ActionResultExtensions
{
    public static T GetResult<T>(this ActionResult<T> actionResult)
    {
        return (T)((OkObjectResult) actionResult.Result!).Value!;
    }
}
