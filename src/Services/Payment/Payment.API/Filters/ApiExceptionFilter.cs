using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;

namespace Payment.API.Filters
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;
        private readonly IWebHostEnvironment _env;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            _logger.LogError(exception, "Unhandled exception occurred");

            var isDevelopment = _env.IsDevelopment();
            
            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "An error occurred while processing your request.",
                Detail = isDevelopment ? context.Exception.StackTrace : null
            };

            // Customize response based on exception type
            if (exception is InvalidOperationException)
            {
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Invalid operation";
                problemDetails.Detail = exception.Message;
            }
            else if (exception is ArgumentException)
            {
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Invalid argument";
                problemDetails.Detail = exception.Message;
            }
            else if (exception is UnauthorizedAccessException)
            {
                problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                problemDetails.Title = "Unauthorized";
                problemDetails.Detail = exception.Message;
            }

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status
            };
            
            context.ExceptionHandled = true;
        }
    }
}