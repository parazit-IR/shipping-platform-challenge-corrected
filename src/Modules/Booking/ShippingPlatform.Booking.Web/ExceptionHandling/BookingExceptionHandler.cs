using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShippingPlatform.Booking.Application.Exception;
using ShippingPlatform.Booking.Application.Port.Out;
using ShippingPlatform.Booking.Domain.Exception;

namespace ShippingPlatform.Booking.Web.ExceptionHandling;

public sealed class BookingExceptionHandler(ILogger<BookingExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            CommercialEligibilityException eligibilityException =>
                eligibilityException.Status switch
                {
                    AgreementEligibilityStatus.CustomerNotFound =>
                        (StatusCodes.Status404NotFound,
                            "Customer not found"),

                    AgreementEligibilityStatus.AgreementNotFound =>
                        (StatusCodes.Status404NotFound,
                            "Agreement not found"),

                    AgreementEligibilityStatus.AgreementInactive =>
                        (StatusCodes.Status422UnprocessableEntity,
                            "Agreement inactive"),

                    _ =>
                        (StatusCodes.Status422UnprocessableEntity,
                            "Agreement not eligible")
                },

            DomainValidationException =>
                (StatusCodes.Status400BadRequest,
                    "Invalid booking request"),

            _ =>
                (StatusCodes.Status500InternalServerError,
                    "Unexpected server error")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing Booking request");
        }
        else
        {
            logger.LogWarning(
                exception,
                "Booking request failed with status code {StatusCode}",
                statusCode);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode >= StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}