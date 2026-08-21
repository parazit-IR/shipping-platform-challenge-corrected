using ShippingPlatform.Domain.Booking.Exception;

namespace ShippingPlatform.Domain.Booking.ValueObject;

public readonly record struct AgreementId
{
    public string Value { get; }

    private AgreementId(string value)
    {
        Value = value;
    }

    public static AgreementId Create(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? throw new DomainValidationException("AgreementId is required.") : new AgreementId(value.Trim());
    }

    public override string ToString() => Value;
}