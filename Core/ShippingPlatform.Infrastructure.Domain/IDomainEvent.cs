namespace ShippingPlatform.Infrastructure.Domain;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}