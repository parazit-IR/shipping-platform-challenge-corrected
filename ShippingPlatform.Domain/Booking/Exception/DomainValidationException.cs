using ShippingPlatform.Infrastructure.Domain;

namespace ShippingPlatform.Domain.Booking.Exception;

public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message)
    {
    }
}