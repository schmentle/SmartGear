using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartGear.PM0902.Filters;

public class AdminHeaderAuthorizeFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var ok = context.HttpContext.Request.Headers.TryGetValue("X-Admin", out var val) &&
                 string.Equals(val.ToString(), "true", StringComparison.OrdinalIgnoreCase);

        if (!ok)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden); 
        }
    }
}