using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShippingPlatform.Booking.Application.Port.In;
using ShippingPlatform.Booking.Web.Contract;

namespace ShippingPlatform.Booking.Web.Controller;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController(ICreateBookingUseCase createBookingUseCase) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateBookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createBookingUseCase.CreateAsync(
            new CreateBookingCommand(
                request.CustomerId!,
                request.AgreementId!,
                request.Origin!,
                request.Destination!,
                request.VoyageId!),
            cancellationToken);

        var response = new CreateBookingResponse(
            result.BookingId,
            result.Status);

        return Created(
            $"/api/bookings/{result.BookingId}",
            response);
    }
}