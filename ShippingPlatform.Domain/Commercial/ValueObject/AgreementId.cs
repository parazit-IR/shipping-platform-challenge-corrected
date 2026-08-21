using ShippingPlatform.Domain.Commercial.Exception;

namespace ShippingPlatform.Domain.Commercial.ValueObject;

public sealed record AgreementId
{
    public string Value { get; }

    private AgreementId(string value)
    {
        Value = value;
    }

    public static AgreementId Create(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new DomainValidationException("AgreementId cannot be empty.")
            : new AgreementId(value.Trim());
    }
}