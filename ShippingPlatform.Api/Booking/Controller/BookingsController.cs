using Microsoft.AspNetCore.Mvc;
using ShippingPlatform.Api.Booking.Contract;
using ShippingPlatform.Application.Booking.Commands.Create;
using ShippingPlatform.Infrastructure.Application;

namespace ShippingPlatform.Api.Booking.Controller;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController : ControllerBase
{
    private readonly ICommandHandler<CreateBookingCommand, CreateBookingResult> _handler;
    public BookingsController(ICommandHandler<CreateBookingCommand, CreateBookingResult> handler)
    {
        _handler = handler;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(CreateBookingResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBookingRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (Request.Headers.ContainsKey("Idempotency-Key"))
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                ModelState.AddModelError(
                    "Idempotency-Key",
                    "The Idempotency-Key header must not be blank.");
                return ValidationProblem(ModelState);
            }

            if (idempotencyKey.Length > 200)
            {
                ModelState.AddModelError(
                    "Idempotency-Key",
                    "The Idempotency-Key header must be 200 characters or fewer.");
                return ValidationProblem(ModelState);
            }
        }

        var result = await _handler.Handle(
            new CreateBookingCommand(
                request.CustomerId!,
                request.AgreementId!,
                request.Origin!,
                request.Destination!,
                request.VoyageId!,
                idempotencyKey?.Trim()),
            cancellationToken);

        return Created(
            $"/api/bookings/{result.BookingId}",
            new CreateBookingResponse(
                result.BookingId,
                result.Status));
    }
}
