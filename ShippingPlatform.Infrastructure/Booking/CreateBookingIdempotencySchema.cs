namespace ShippingPlatform.Infrastructure.Booking;

public static class CreateBookingIdempotencySchema
{
    public const string TableName = "create_booking_idempotency";
    public const string SchemaName = "booking";
    public const string PrimaryKeyName = "PK_create_booking_idempotency";
}
