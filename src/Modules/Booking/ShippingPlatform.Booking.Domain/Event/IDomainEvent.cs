namespace ShippingPlatform.Booking.Domain.Event;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}