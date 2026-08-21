using ShippingPlatform.Domain.Booking;
using ShippingPlatform.Infrastructure.DataAccess;

using BookingAggregate = ShippingPlatform.Domain.Booking.Entity.Booking;

namespace ShippingPlatform.Infrastructure.Booking;

public sealed class BookingReadRepository : ReadRepository<BookingAggregate>, IBookingReadRepository
{
    public BookingReadRepository(Context context) : base(context)
    {
    }
}