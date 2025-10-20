using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace SmartGear.PM0902.Filters;

public class LogActionFilter : IActionFilter
{
    private readonly ILogger<LogActionFilter> _logger;
    public LogActionFilter(ILogger<LogActionFilter> logger) => _logger = logger;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var route = context.HttpContext.Request.Path;
        _logger.LogInformation("➡️ Executing {Action} ({Route})",
            context.ActionDescriptor.DisplayName, route);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var route = context.HttpContext.Request.Path;
        var status = context.HttpContext.Response.StatusCode;
        _logger.LogInformation("✅ Executed {Action} ({Route}) Status={Status}",
            context.ActionDescriptor.DisplayName, route, status);
    }
}