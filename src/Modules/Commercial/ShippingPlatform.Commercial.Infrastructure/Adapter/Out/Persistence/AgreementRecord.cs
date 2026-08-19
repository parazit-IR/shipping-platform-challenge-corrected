namespace ShippingPlatform.Commercial.Infrastructure.Adapter.Out.Persistence;

public sealed class AgreementRecord
{
    public required string AgreementId { get; init; }
    public required string CustomerId { get; init; }
    public required string Status { get; init; }
}