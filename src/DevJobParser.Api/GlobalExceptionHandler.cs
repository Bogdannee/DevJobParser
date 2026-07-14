using System.Net;
using DevJobParser.Core.Exceptions; // Твои кастомные исключения
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DevJobParser.Api
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            var problemDetails = exception switch
            {
                HtmlPageLoadingException htmlEx => new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadGateway,
                    Title = "External Site Loading Failed",
                    Detail = $"Could not load page from: {htmlEx.Url}. Error: {htmlEx.Message}"
                },
                JobParsingException parseEx => new ProblemDetails
                {
                    Status = (int)HttpStatusCode.UnprocessableEntity, // 422
                    Title = "Job Parsing Failed",
                    Detail = $"Failed to parse details for job: {parseEx.JobUrl}. Error: {parseEx.Message}"
                },
                ArgumentException argEx => new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest, // 400
                    Title = "Invalid Argument or Configuration",
                    Detail = argEx.Message
                },
                _ => new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError, // 500
                    Title = "An unexpected error occurred",
                    Detail = "Internal server error. Please try again later."
                }
            };

            httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}