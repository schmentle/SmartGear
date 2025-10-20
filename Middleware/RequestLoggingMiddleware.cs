using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using SmartGear.PM0902.Services;

namespace SmartGear.PM0902.Middleware;

public sealed class RequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly IRequestTimeService _timeService;

    public RequestLoggingMiddleware(
        ILogger<RequestLoggingMiddleware> logger,
        IRequestTimeService timeService)
    {
        _logger = logger;
        _timeService = timeService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var sw = Stopwatch.StartNew();
        var path = context.Request.Path.Value ?? "/";
        var method = context.Request.Method;

        // Attach a correlation id and request timestamp
        var correlationId = context.TraceIdentifier;
        context.Response.Headers["x-correlation-id"] = correlationId;

        _logger.LogInformation(
            "➡️  {Method} {Path} at {Utc} (corrId={CorrelationId})",
            method, path, _timeService.UtcNow, correlationId);

        await next(context);

        sw.Stop();
        _logger.LogInformation(
            "✅  {Method} {Path} completed {StatusCode} in {Elapsed} ms (corrId={CorrelationId})",
            method, path, context.Response.StatusCode, sw.ElapsedMilliseconds, correlationId);
    }
}