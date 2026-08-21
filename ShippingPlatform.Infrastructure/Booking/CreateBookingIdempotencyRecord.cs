namespace ShippingPlatform.Infrastructure.Booking;

internal sealed class CreateBookingIdempotencyRecord
{
    public string IdempotencyKey { get; private set; } = null!;
    public string RequestFingerprint { get; private set; } = null!;
    public CreateBookingIdempotencyState State { get; private set; }
    public Guid? BookingId { get; private set; }
    public string? BookingStatus { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private CreateBookingIdempotencyRecord()
    {
    }

    public static CreateBookingIdempotencyRecord CreatePending(
        string idempotencyKey,
        string requestFingerprint,
        DateTimeOffset createdAt)
    {
        return new CreateBookingIdempotencyRecord
        {
            IdempotencyKey = idempotencyKey,
            RequestFingerprint = requestFingerprint,
            State = CreateBookingIdempotencyState.Pending,
            CreatedAt = createdAt
        };
    }

    public void Complete(
        Guid bookingId,
        string bookingStatus,
        DateTimeOffset completedAt)
    {
        BookingId = bookingId;
        BookingStatus = bookingStatus;
        CompletedAt = completedAt;
        State = CreateBookingIdempotencyState.Completed;
    }
}
