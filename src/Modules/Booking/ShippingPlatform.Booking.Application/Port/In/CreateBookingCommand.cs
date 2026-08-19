namespace ShippingPlatform.Booking.Application.Port.In;

public sealed record CreateBookingCommand(
    string CustomerId,
    string AgreementId,
    string Origin,
    string Destination,
    string VoyageId);