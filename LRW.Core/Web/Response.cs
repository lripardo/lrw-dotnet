using System.Net;

namespace LRW.Core.Web;

public class Response(HttpStatusCode status = HttpStatusCode.OK, string message = "", object? data = null, string code = "")
{
    public HttpStatusCode Status { get; } = status;
    public string Code { get; } = code;
    public string Message { get; } = message;
    public object? Data { get; } = data;
}
