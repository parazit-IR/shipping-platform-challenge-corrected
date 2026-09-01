using MediatR;
using ShippingPlatform.Domain.Booking.ValueObject;
using ShippingPlatform.Domain.DataAccess;
using BookingAggregate = ShippingPlatform.Domain.Booking.Entity.Booking;


namespace ShippingPlatform.IntegrationTests.Infrastructure;

public class CreateBookingThenFailHandler: IRequestHandler<CreateBookingThenFailCommand, bool>
{
    private readonly IWriteUnitOfWork _writeUnitOfWork;

    public CreateBookingThenFailHandler(IWriteUnitOfWork writeUnitOfWork)
    {
        _writeUnitOfWork = writeUnitOfWork;
    }
    
    public async Task<bool> Handle(
        CreateBookingThenFailCommand request,
        CancellationToken cancellationToken)
    {
        var booking = BookingAggregate.Create(
            CustomerId.Create("CUSTOMER-TX-TEST"),
            AgreementId.Create("AGREEMENT-TX-TEST"),
            Origin.Create("IRBND"),
            Destination.Create("DEHAM"),
            VoyageId.Create("VOYAGE-TX-TEST"));

        await _writeUnitOfWork.Bookings.AddAsync(booking, cancellationToken);
        await _writeUnitOfWork.SaveChangesAsync(cancellationToken);
        throw new InvalidOperationException("Force rollback");
    }
}