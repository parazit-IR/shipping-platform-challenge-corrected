using ShippingPlatform.Domain.Booking;
using ShippingPlatform.Infrastructure.DataAccess;

using BookingAggregate = ShippingPlatform.Domain.Booking.Entity.Booking;

namespace ShippingPlatform.Infrastructure.Booking;

public sealed class BookingWriteRepository : WriteRepository<BookingAggregate>, IBookingWriteRepository
{
    public BookingWriteRepository(Context context) : base(context)
    {
    }
}