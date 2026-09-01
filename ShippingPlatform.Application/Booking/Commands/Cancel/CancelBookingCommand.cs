using ShippingPlatform.Domain.Booking.ValueObject;
using ShippingPlatform.Infrastructure.Application;

namespace ShippingPlatform.Application.Booking.Commands.Cancel;

public sealed record CancelBookingCommand(Guid BookingId) : ITransactionalCommand<CancelBookingResult>;