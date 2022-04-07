using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
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
}
