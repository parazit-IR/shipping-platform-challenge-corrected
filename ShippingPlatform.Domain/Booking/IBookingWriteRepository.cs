using ShippingPlatform.Infrastructure.DataAccess.Domain;


namespace ShippingPlatform.Domain.Booking;

public interface IBookingWriteRepository : IWriteRepository<Entity.Booking>
{
}