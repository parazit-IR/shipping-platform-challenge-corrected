namespace ShippingPlatform.Application.Booking.Commands.Create;

public interface ICreateBookingIdempotencyExecutor
{
    Task<CreateBookingResult> ExecuteAsync(
        string idempotencyKey,
        string requestFingerprint,
        Func<CancellationToken, Task<CreateBookingResult>> operation,
        CancellationToken cancellationToken = default);
}
