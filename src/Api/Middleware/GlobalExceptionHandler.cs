using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, extensions) = Map(exception);

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path,
        };

        foreach (var (key, value) in extensions)
        {
            problemDetails.Extensions[key] = value;
        }

        httpContext.Response.StatusCode = (int)statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (HttpStatusCode StatusCode, string Title, Dictionary<string, object?> Extensions) Map(Exception exception) =>
        exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "خطای اعتبارسنجی",
                new Dictionary<string, object?>
                {
                    ["errors"] = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
                }),
            NotFoundException => (HttpStatusCode.NotFound, "یافت نشد", []),
            ConflictException => (HttpStatusCode.Conflict, "تعارض وضعیت", []),
            UnauthorizedDomainException => (HttpStatusCode.Forbidden, "دسترسی غیرمجاز", []),
            RateLimitExceededException rateLimitExceeded => (
                (HttpStatusCode)429,
                "تعداد درخواست بیش از حد مجاز",
                rateLimitExceeded.RetryAfter is { } retryAfter
                    ? new Dictionary<string, object?> { ["retryAfterSeconds"] = (int)retryAfter.TotalSeconds }
                    : []),
            DomainException => (HttpStatusCode.BadRequest, "درخواست نامعتبر", []),
            _ => (HttpStatusCode.InternalServerError, "خطای غیرمنتظره سرور", []),
        };
}
