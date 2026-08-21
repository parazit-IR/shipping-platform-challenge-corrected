using ShippingPlatform.Domain.Booking.Exception;

namespace ShippingPlatform.Domain.Booking.ValueObject;

public readonly record struct VoyageId
{
    public string Value { get; }

    private VoyageId(string value)
    {
        Value = value;
    }

    public static VoyageId Create(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? throw new DomainValidationException("VoyageId is required.") : new VoyageId(value.Trim());
    }

    public override string ToString() => Value;
}