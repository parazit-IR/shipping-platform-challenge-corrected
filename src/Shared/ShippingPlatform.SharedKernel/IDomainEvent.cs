namespace ShippingPlatform.SharedKernel;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}