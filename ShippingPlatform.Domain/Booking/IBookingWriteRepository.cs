using ShippingPlatform.Domain.Booking.ValueObject;
using ShippingPlatform.Infrastructure.DataAccess.Domain;


namespace ShippingPlatform.Domain.Booking;

public interface IBookingWriteRepository : IWriteRepository<Entity.Booking>
{
    Task<Entity.Booking?> FindByIdAsync(
        BookingId bookingId,
        CancellationToken cancellationToken = default);
}