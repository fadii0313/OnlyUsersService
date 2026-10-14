using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using OnlyUsersService.API.Models.ResponseModels.BaseModels;

namespace OnlyUsersService.API.Common
{
    public class GlobalActivityHandler : IActionFilter
    {
        private readonly ILogger<GlobalActivityHandler> _logger;
        private Stopwatch _stopwatch;

        public GlobalActivityHandler(ILogger<GlobalActivityHandler> logger = null)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _stopwatch = Stopwatch.StartNew();
            _logger?.LogDebug("Executing {Action} on {Path}", context.ActionDescriptor.DisplayName, context.HttpContext.Request.Path);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _stopwatch?.Stop();
            var elapsedMs = _stopwatch?.ElapsedMilliseconds ?? 0;

            if (context.Result is Microsoft.AspNetCore.Mvc.ObjectResult objectResult &&
                objectResult.Value is BaseResponseModel baseResponse)
            {
                baseResponse.ResponseExecutionTime = $"{elapsedMs}ms";
            }

            _logger?.LogDebug("Executed {Action} in {ElapsedMs}ms", context.ActionDescriptor.DisplayName, elapsedMs);
        }
    }
}
