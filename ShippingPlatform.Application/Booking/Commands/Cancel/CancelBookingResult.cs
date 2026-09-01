using ShippingPlatform.Domain.Booking.ValueObject;

namespace ShippingPlatform.Application.Booking.Commands.Cancel;

public sealed record CancelBookingResult(Guid BookingId, string Status);