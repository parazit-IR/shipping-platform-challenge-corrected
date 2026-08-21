namespace ShippingPlatform.Application.Booking.Commands.Create;

public sealed record CreateBookingResult(
    string BookingId,
    string Status);