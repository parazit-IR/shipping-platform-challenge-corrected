namespace ShippingPlatform.Infrastructure.Booking;

internal enum CreateBookingIdempotencyState
{
    Pending = 1,
    Completed = 2
}
