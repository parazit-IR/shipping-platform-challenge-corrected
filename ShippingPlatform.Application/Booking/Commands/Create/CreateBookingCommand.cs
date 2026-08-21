using ShippingPlatform.Infrastructure.Application;

namespace ShippingPlatform.Application.Booking.Commands.Create;

public sealed record CreateBookingCommand(
    string CustomerId,
    string AgreementId,
    string Origin,
    string Destination,
    string VoyageId)
    : ICommand<CreateBookingResult>;