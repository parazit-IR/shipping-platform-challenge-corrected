using Microsoft.EntityFrameworkCore;
using ShippingPlatform.Application.Booking.Commands.Create;
using ShippingPlatform.Application.Exceptions;

namespace ShippingPlatform.Infrastructure.Booking;

public sealed class CreateBookingIdempotencyExecutor
    : ICreateBookingIdempotencyExecutor
{
    private readonly Context _context;

    public CreateBookingIdempotencyExecutor(Context context)
    {
        _context = context;
    }

    public async Task<CreateBookingResult> ExecuteAsync(
        string idempotencyKey,
        string requestFingerprint,
        Func<CancellationToken, Task<CreateBookingResult>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var reservation =
                CreateBookingIdempotencyRecord.CreatePending(
                    idempotencyKey,
                    requestFingerprint,
                    DateTimeOffset.UtcNow);

            _context.Add(reservation);

            await _context.SaveChangesAsync(cancellationToken);

            var result = await operation(cancellationToken);

            reservation.Complete(
                Guid.Parse(result.BookingId),
                result.Status,
                DateTimeOffset.UtcNow);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch (DbUpdateException exception)
            when (CreateBookingIdempotencyErrorClassifier.IsIdempotencyKeyUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();

            return await ReplayOrThrowConflictAsync(
                idempotencyKey,
                requestFingerprint,
                cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<CreateBookingResult> ReplayOrThrowConflictAsync(
        string idempotencyKey,
        string requestFingerprint,
        CancellationToken cancellationToken)
    {
        var record =
            await _context.Set<CreateBookingIdempotencyRecord>()
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.IdempotencyKey == idempotencyKey,
                    cancellationToken)
            ?? throw new InvalidOperationException(
                $"Idempotency record '{idempotencyKey}' was not found after a uniqueness conflict.");

        if (!string.Equals(record.RequestFingerprint, requestFingerprint, StringComparison.Ordinal))
        {
            throw new IdempotencyConflictException(idempotencyKey);
        }

        if (record.State != CreateBookingIdempotencyState.Completed ||
            record.BookingId is null ||
            string.IsNullOrWhiteSpace(record.BookingStatus))
        {
            throw new InvalidOperationException(
                $"Idempotency record '{idempotencyKey}' is not in a replayable state.");
        }

        return new CreateBookingResult(
            record.BookingId.Value.ToString(),
            record.BookingStatus);
    }
}
