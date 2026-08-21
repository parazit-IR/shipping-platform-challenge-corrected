using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ShippingPlatform.Infrastructure.Booking;

public static class CreateBookingIdempotencyErrorClassifier
{
    public static bool IsIdempotencyKeyUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException &&
               postgresException.SqlState == PostgresErrorCodes.UniqueViolation &&
               string.Equals(
                   postgresException.ConstraintName,
                   CreateBookingIdempotencySchema.PrimaryKeyName,
                   StringComparison.Ordinal);
    }
}
