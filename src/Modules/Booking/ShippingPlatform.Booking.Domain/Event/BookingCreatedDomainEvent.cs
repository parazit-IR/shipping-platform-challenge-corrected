using ShippingPlatform.SharedKernel;

namespace ShippingPlatform.Booking.Domain.Event;

public sealed record BookingCreatedDomainEvent(
    string BookingId,
    string CustomerId,
    string AgreementId,
    string Origin,
    string Destination,
    string VoyageId,
    DateTimeOffset OccurredAt): IDomainEvent
{
}