using Microsoft.AspNetCore.Mvc.Filters;

namespace BoltonCup.WebAPI.Filters;

public class ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> _logger, ISentryClient _sentry) : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception for request {DisplayName}", context.ActionDescriptor.DisplayName);
        _sentry.CaptureException(context.Exception);
        base.OnException(context);
    }
}