namespace ShippingPlatform.Booking.Domain.Exception;

public class BookingDomainException(string message) : SystemException(message);