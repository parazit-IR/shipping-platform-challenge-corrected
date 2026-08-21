using ShippingPlatform.Domain.Booking.Exception;

namespace ShippingPlatform.Domain.Booking.ValueObject;

public readonly record struct Origin
{
    public string Value { get; }

    private Origin(string value)
    {
        Value = value;
    }

    public static Origin Create(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? throw new DomainValidationException("Origin is required.") : new Origin(value.Trim());
    }

    public override string ToString() => Value;
}