namespace ShippingPlatform.Booking.Application.Port.In;

public interface ICreateBookingUseCase
{
    Task<CreateBookingResult> CreateAsync(CreateBookingCommand command, CancellationToken cancellationToken);
}