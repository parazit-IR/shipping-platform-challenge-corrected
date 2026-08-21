using ShippingPlatform.Domain.Booking;
using ShippingPlatform.Domain.Commercial;
using ShippingPlatform.Domain.DataAccess;
using ShippingPlatform.Infrastructure.Booking;
using ShippingPlatform.Infrastructure.Commercial;
using ShippingPlatform.Infrastructure.DataAccess;

namespace ShippingPlatform.Infrastructure;

public sealed class ReadUnitOfWork : BaseReadUnitOfWork, IReadUnitOfWork
{
    public ReadUnitOfWork(Context context) : base(context)
    {
        Bookings = new BookingReadRepository(context);
        Agreements = new AgreementReadRepository(context);
    }

    public IBookingReadRepository Bookings { get; }

    public IAgreementReadRepository Agreements { get; }
}