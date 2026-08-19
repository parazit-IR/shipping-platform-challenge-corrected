namespace ShippingPlatform.Booking.Application.Port.Out;

using BookingAggregate = ShippingPlatform.Booking.Domain.Entity.Booking;

public interface IBookingRepository
{
    Task AddAsync(BookingAggregate booking, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}