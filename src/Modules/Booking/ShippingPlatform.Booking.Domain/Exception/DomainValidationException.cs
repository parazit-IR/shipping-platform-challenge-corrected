namespace ShippingPlatform.Booking.Domain.Exception;

public class DomainValidationException(string message): SystemException(message)
{
}