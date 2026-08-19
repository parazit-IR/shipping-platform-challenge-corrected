namespace ShippingPlatform.Booking.Domain.Exception;

public sealed class DomainValidationException(string message): BookingDomainException(message);