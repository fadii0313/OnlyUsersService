using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using OnlyUsersService.API.Models.ResponseModels.BaseModels;

namespace OnlyUsersService.API.Common
{
    public class GlobalExceptionHandler : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger = null)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger?.LogError(context.Exception, "Unhandled exception caught by GlobalExceptionHandler: {Message}", context.Exception.Message);

            var response = new BaseResponseModel();
            response.ExceptionModel(context.Exception);

            context.Result = new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            context.ExceptionHandled = true;
        }
    }
}
