using System.Collections.Generic;

namespace Trakx.Utils.Apis;

public partial class Response<TResult> : Response
{
    public TResult Result { get; private set; }

    public Response(int statusCode, IReadOnlyDictionary<string, IEnumerable<string>> headers, TResult result)
        : base(statusCode, headers)
    {
        Result = result;
    }
}

public partial class Response
{
    public int StatusCode { get; private set; }

    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; private set; }

    public Response(int statusCode, IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        StatusCode = statusCode;
        Headers = headers;
    }
}
