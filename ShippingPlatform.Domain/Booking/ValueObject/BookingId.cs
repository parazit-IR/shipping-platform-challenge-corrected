namespace ShippingPlatform.Domain.Booking.ValueObject;

public readonly record struct BookingId(Guid Value)
{
    public static BookingId Create()
    {
        return new BookingId(Guid.NewGuid());
    }
}