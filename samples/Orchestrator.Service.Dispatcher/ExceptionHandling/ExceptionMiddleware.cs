using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleWebApi.Models;

namespace SampleWebApi.ExceptionHandling
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger logger)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex.Message);
                await HandleExceptionAsync(httpContext).ConfigureAwait(false);
            }
        }

        private Task HandleExceptionAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.WriteAsync(new ErrorDetails
                    {
                        RequestStatus = context.Response.StatusCode,
                        Message = "An unexpected error occured"
                    }
                    .ToString())
                .ConfigureAwait(false);

            return Task.CompletedTask;
        }
    }
}
