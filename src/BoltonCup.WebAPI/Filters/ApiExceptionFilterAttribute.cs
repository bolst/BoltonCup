using Microsoft.AspNetCore.Mvc.Filters;

namespace BoltonCup.WebAPI.Filters;

public class ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> _logger) : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        // just log for now
        _logger.LogError(context.Exception, "Unhandled exception for request {DisplayName}", context.ActionDescriptor.DisplayName);
        base.OnException(context);
    }
}