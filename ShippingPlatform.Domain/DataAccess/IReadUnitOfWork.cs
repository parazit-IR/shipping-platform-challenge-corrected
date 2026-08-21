using ShippingPlatform.Domain.Booking;
using ShippingPlatform.Domain.Commercial;
using ShippingPlatform.Infrastructure.DataAccess.Domain;

namespace ShippingPlatform.Domain.DataAccess;

public interface IReadUnitOfWork : IBaseReadUnitOfWork
{
    IBookingReadRepository Bookings { get; }

    IAgreementReadRepository Agreements { get; }
}