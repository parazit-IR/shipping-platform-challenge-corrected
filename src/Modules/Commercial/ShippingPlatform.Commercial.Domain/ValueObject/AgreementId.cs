using ShippingPlatform.Commercial.Domain.Exception;

namespace ShippingPlatform.Commercial.Domain.ValueObject;

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