using ShippingPlatform.Domain.Commercial.Exception;

namespace ShippingPlatform.Domain.Commercial.ValueObject;

public sealed record CustomerId
{
    public string Value { get; }

    private CustomerId(string value)
    {
        Value = value;
    }

    public static CustomerId Create(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new DomainValidationException("CustomerId cannot be empty.")
            : new CustomerId(value.Trim());
    }
}