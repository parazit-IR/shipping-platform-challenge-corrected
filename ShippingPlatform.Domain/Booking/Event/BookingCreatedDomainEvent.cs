using ShippingPlatform.Infrastructure.Domain;

namespace ShippingPlatform.Domain.Booking.Event;

public sealed record BookingCreatedDomainEvent(
    string BookingId,
    string CustomerId,
    string AgreementId,
    string Origin,
    string Destination,
    string VoyageId,
    DateTimeOffset OccurredAt) : IDomainEvent;