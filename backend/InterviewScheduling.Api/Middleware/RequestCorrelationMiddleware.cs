using Microsoft.Extensions.Primitives;

namespace InterviewScheduling.Api.Middleware;

/// <summary>
/// Propagates or generates <c>X-Request-Id</c>, echoes it on the response, and adds a logging scope so
/// structured logs for the request share the same correlation key.
/// </summary>
public sealed class RequestCorrelationMiddleware(RequestDelegate next)
{
    public const string RequestIdHeaderName = "X-Request-Id";
    public const string RequestIdItemKey = "RequestId";

    public async Task InvokeAsync(HttpContext context, ILogger<RequestCorrelationMiddleware> logger)
    {
        var requestId = context.Request.Headers.TryGetValue(RequestIdHeaderName, out StringValues incoming) &&
                        !StringValues.IsNullOrEmpty(incoming)
            ? incoming.ToString()
            : Guid.NewGuid().ToString("N");

        context.Items[RequestIdItemKey] = requestId;
        context.Response.Headers.Append(RequestIdHeaderName, requestId);

        using (logger.BeginScope(new Dictionary<string, object?> { [RequestIdItemKey] = requestId }))
            await next(context);
    }
}
