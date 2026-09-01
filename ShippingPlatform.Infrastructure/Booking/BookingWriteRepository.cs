using Microsoft.EntityFrameworkCore;
using ShippingPlatform.Domain.Booking;
using ShippingPlatform.Domain.Booking.ValueObject;
using ShippingPlatform.Infrastructure.DataAccess;

using BookingAggregate = ShippingPlatform.Domain.Booking.Entity.Booking;

namespace ShippingPlatform.Infrastructure.Booking;

public sealed class BookingWriteRepository : WriteRepository<BookingAggregate>, IBookingWriteRepository
{
    public BookingWriteRepository(Context context) : base(context)
    {
    }
    
    public Task<BookingAggregate?> FindByIdAsync(BookingId bookingId, CancellationToken cancellationToken = default)
    {
        return DbSet.SingleOrDefaultAsync(x => x.Id == bookingId, cancellationToken);
    }
}