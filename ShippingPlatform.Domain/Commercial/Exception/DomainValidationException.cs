using ShippingPlatform.Infrastructure.Domain;

namespace ShippingPlatform.Domain.Commercial.Exception;

public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message)
    {
    }
}