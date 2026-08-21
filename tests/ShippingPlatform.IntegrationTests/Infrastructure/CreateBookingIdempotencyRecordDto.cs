namespace ShippingPlatform.IntegrationTests.Infrastructure;

public sealed record CreateBookingIdempotencyRecordDto(
    string IdempotencyKey,
    string RequestFingerprint,
    string State,
    Guid? BookingId,
    string? BookingStatus);
