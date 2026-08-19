namespace ShippingPlatform.Booking.Domain.ValueObject;

public enum BookingStatus
{
    Draft = 1,
    QuotationRequested = 2,
    Quoted = 3,
    PendingCapacity = 4,
    PendingPayment = 5,
    Confirmed = 6,
    Cancelled = 7,
    Completed = 8
}