using ShippingPlatform.Booking.Domain.Exception;

namespace ShippingPlatform.Booking.Domain.ValueObject;

public readonly record struct Destination
{
    public string Value { get; }

    private Destination(string value)
    {
        Value = value;
    }

    public static Destination Create(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? throw new DomainValidationException("Destination is required.") : new Destination(value.Trim());
    }

    public override string ToString() => Value;
}