using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;
using System.Text.Json;

namespace SimilarityDemo.Infrastructure;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception has occurred while executing the request.");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        httpContext.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            traceId,
            StatusCode = StatusCodes.Status500InternalServerError,
        }, jsonSerializerOptions), cancellationToken);

        return ValueTask.FromResult(true);
    }
}
