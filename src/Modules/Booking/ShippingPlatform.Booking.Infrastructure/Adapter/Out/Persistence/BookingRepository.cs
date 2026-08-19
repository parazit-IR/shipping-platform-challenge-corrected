using Microsoft.EntityFrameworkCore;
using ShippingPlatform.Booking.Application.Port.Out;
using ShippingPlatform.Booking.Infrastructure.Adapter.Out.Persistence.Entity;

namespace ShippingPlatform.Booking.Infrastructure.Adapter.Out.Persistence;

public sealed class BookingRepository(BookingDbContext dbContext): IBookingRepository
{
    public Task AddAsync(Domain.Entity.Booking booking, CancellationToken cancellationToken)
    {
        var record = new BookingRecord
        {
            BookingId = booking.Id.Value,
            CustomerId = booking.CustomerId.Value,
            AgreementId = booking.AgreementId.Value,
            Origin = booking.Origin.Value,
            Destination = booking.Destination.Value,
            VoyageId = booking.VoyageId.Value,
            Status = booking.Status.ToString(),
            CreatedAt = booking.CreatedAt
        };

        return dbContext.Bookings.AddAsync(record, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}