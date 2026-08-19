namespace ShippingPlatform.Booking.Infrastructure.Adapter.Out.Persistence.Entity;

public sealed class BookingRecord
{
    public required Guid BookingId { get; init; }

    public required string CustomerId { get; init; }

    public required string AgreementId { get; init; }

    public required string Origin { get; init; }

    public required string Destination { get; init; }

    public required string VoyageId { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}