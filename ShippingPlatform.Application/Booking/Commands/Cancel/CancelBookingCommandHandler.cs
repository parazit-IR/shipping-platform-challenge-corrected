using MediatR;
using ShippingPlatform.Domain.Booking.ValueObject;
using ShippingPlatform.Domain.DataAccess;
using ShippingPlatform.Infrastructure.Application;

namespace ShippingPlatform.Application.Booking.Commands.Cancel;

public class CancelBookingCommandHandler : ICommandHandler<CancelBookingCommand, CancelBookingResult>
{
    private readonly IWriteUnitOfWork _unitOfWork;

    public CancelBookingCommandHandler(IWriteUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    async Task<CancelBookingResult> IRequestHandler<CancelBookingCommand, CancelBookingResult>.Handle(
        CancelBookingCommand command, CancellationToken cancellationToken)
    {
        var bookingId = new BookingId(command.BookingId);

        // get entity without no tracking (dirty checking)
        var booking = await _unitOfWork.Bookings.FindByIdAsync(
            bookingId,
            cancellationToken);

        if (booking is null)
        {
            throw new InvalidOperationException($"Booking '{command.BookingId}' was not found.");
        }

        booking.Cancel();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CancelBookingResult(booking.Id.Value, booking.Status.ToString());
    }

    Task<CancelBookingResult> ICommandHandler<CancelBookingCommand, CancelBookingResult>.Handle(
        CancelBookingCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}