namespace ShippingPlatform.Booking.Application.Port.In;

public sealed record CreateBookingResult(
    string BookingId,
    string Status);