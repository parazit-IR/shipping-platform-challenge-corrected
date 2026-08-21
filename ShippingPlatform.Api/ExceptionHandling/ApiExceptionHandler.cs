using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ShippingPlatform.Application.Commercial.Queries.CheckAgreementEligibility;
using ShippingPlatform.Application.Exceptions;
using ShippingPlatform.Infrastructure.Domain;

namespace ShippingPlatform.Api.ExceptionHandling;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            CommercialEligibilityException eligibility =>
                eligibility.Status switch
                {
                    CommercialEligibilityStatus.CustomerNotFound =>
                        (404, "Customer not found"),

                    CommercialEligibilityStatus.AgreementNotFound =>
                        (404, "Agreement not found"),

                    CommercialEligibilityStatus.AgreementInactive =>
                        (422, "Agreement inactive"),

                    _ =>
                        (422, "Agreement not eligible")
                },

            IdempotencyConflictException =>
                (409, "Idempotency key conflict"),

            DomainException =>
                (400, "Invalid request"),

            _ =>
                (500, "Unexpected server error")
        };

        if (statusCode >= 500)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogWarning(
                exception,
                "Request failed with status {StatusCode}",
                statusCode);

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode >= 500
                    ? "An unexpected error occurred."
                    : exception.Message
            },
            cancellationToken);

        return true;
    }
}
