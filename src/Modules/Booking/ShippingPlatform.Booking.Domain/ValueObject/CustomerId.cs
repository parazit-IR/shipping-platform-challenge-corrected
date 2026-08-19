using ShippingPlatform.Booking.Domain.Exception;

namespace ShippingPlatform.Booking.Domain.ValueObject;

public readonly record struct CustomerId
{
    public string Value { get; }

    private CustomerId(string value)
    {
        Value = value;
    }

    public static CustomerId Create(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? throw new DomainValidationException("CustomerId is required.") : new CustomerId(value.Trim());
    }

    public override string ToString() => Value;
}