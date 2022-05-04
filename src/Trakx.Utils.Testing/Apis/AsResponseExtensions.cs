using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trakx.Utils.Apis;

namespace Trakx.Utils.Testing.Apis;

public static class AsResponseExtensions
{
    public static Response<T> AsResponse<T>(this T result, int? statusCode = default,
        IReadOnlyDictionary<string, IEnumerable<string>>? headers = null)
    {
        statusCode ??= StatusCodes.Status200OK;
        headers ??= new Dictionary<string, IEnumerable<string>>();
        var response = new Response<T>(statusCode.Value, headers, result);
        return response;
    }

    /// <summary>
    ///Extract the content of the ActionResult.
    /// </summary>
    /// <param name="actionResult">Typically the response from a controller.</param>
    /// <typeparam name="T">The type of the content of the ActionResult response.</typeparam>
    public static T GetResultAs<T>(this ActionResult<T> actionResult)
    {
        var result = (T)((ObjectResult)actionResult.Result!).Value!;
        return result;
    }

    /// <summary>
    ///Extract the content of the ActionResult.
    /// </summary>
    /// <param name="actionResult">Typically the response from a controller.</param>
    /// <typeparam name="T">The type of the content of the ActionResult response.</typeparam>
    /// <typeparam name="TExpectedObjectResultType">The expected typed of the ObjectResult (ex. OkObjectResult, BadObjectResult, etc.)</typeparam>
    public static T GetResultAs<T, TExpectedObjectResultType>(this ActionResult<T> actionResult)
        where TExpectedObjectResultType : ObjectResult
    {
        actionResult.Result.Should().BeOfType<TExpectedObjectResultType>();
        var result = (T)((TExpectedObjectResultType)actionResult.Result!).Value!;
        return result;
    }
}
