using ShippingPlatform.Booking.Domain.Exception;

namespace ShippingPlatform.Booking.Domain.ValueObject;

public sealed record ContainerRequest
{
    public string ContainerType { get; }
    public string ContainerSize { get; }
    public int Quantity { get; }
    public decimal Weight { get; }

    private ContainerRequest(
        string containerType,
        string containerSize,
        int quantity,
        decimal weight)
    {
        ContainerType = containerType;
        ContainerSize = containerSize;
        Quantity = quantity;
        Weight = weight;
    }

    public static ContainerRequest Create(
        string containerType,
        string containerSize,
        int quantity,
        decimal weight)
    {
        if (string.IsNullOrWhiteSpace(containerType))
        {
            throw new DomainValidationException("Container type is required.");
        }

        if (string.IsNullOrWhiteSpace(containerSize))
        {
            throw new DomainValidationException("Container size is required.");
        }

        if (quantity <= 0)
        {
            throw new DomainValidationException("Container quantity must be greater than zero.");
        }

        if (weight < 0)
        {
            throw new DomainValidationException("Container weight cannot be negative.");
        }

        return new ContainerRequest(
            containerType.Trim(),
            containerSize.Trim(),
            quantity,
            weight);
    }
}