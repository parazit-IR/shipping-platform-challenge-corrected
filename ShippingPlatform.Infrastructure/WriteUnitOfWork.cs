using ShippingPlatform.Domain.Booking;
using ShippingPlatform.Domain.Commercial;
using ShippingPlatform.Domain.DataAccess;
using ShippingPlatform.Infrastructure.Booking;
using ShippingPlatform.Infrastructure.Commercial;
using ShippingPlatform.Infrastructure.DataAccess;

namespace ShippingPlatform.Infrastructure;

public sealed class WriteUnitOfWork : BaseUnitOfWork, IWriteUnitOfWork
{
    public WriteUnitOfWork(Context context) : base(context)
    {
        Bookings = new BookingWriteRepository(context);
        Agreements = new AgreementWriteRepository(context);
    }

    public IBookingWriteRepository Bookings { get; }

    public IAgreementWriteRepository Agreements { get; }
}