using ShippingPlatform.Domain.Booking;
using ShippingPlatform.Domain.Commercial;
using ShippingPlatform.Infrastructure.DataAccess.Domain;

namespace ShippingPlatform.Domain.DataAccess;

public interface IWriteUnitOfWork : IBaseUnitOfWork
{
    IBookingWriteRepository Bookings { get; }

    IAgreementWriteRepository Agreements { get; }
}