using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SignalChat.Backend.Exceptions;
using ValidationException = SignalChat.Backend.Exceptions.ValidationException;

namespace SignalChat.Backend.Middleware;

public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        context.Response.StatusCode = exception switch
        {
            AppException ex => ex.StatusCode,
            _ => StatusCodes.Status500InternalServerError
        };

        ProblemDetails problemDetails = exception is ValidationException validationEx
            ? new ValidationProblemDetails(
                validationEx.Failures
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(f => f.ErrorMessage).ToArray()))
            {
                Status = context.Response.StatusCode,
                Title = validationEx.Message
            }
            : new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = exception is AppException e ? e.Message : "Internal Server Error"
            };

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }
}
