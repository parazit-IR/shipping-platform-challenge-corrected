using Microsoft.EntityFrameworkCore;
using Npgsql;
using ShippingPlatform.Infrastructure.Booking;

namespace ShippingPlatform.IntegrationTests.Infrastructure;

public sealed class CreateBookingIdempotencyErrorClassifierTests
{
    [Fact]
    public void IsIdempotencyKeyUniqueViolation_ShouldReturnTrue_ForIdempotencyPrimaryKeyConstraint()
    {
        var exception = CreateDbUpdateException(
            PostgresErrorCodes.UniqueViolation,
            CreateBookingIdempotencySchema.PrimaryKeyName);

        Assert.True(CreateBookingIdempotencyErrorClassifier.IsIdempotencyKeyUniqueViolation(exception));
    }

    [Fact]
    public void IsIdempotencyKeyUniqueViolation_ShouldReturnFalse_ForOtherUniqueConstraint()
    {
        var exception = CreateDbUpdateException(
            PostgresErrorCodes.UniqueViolation,
            "PK_some_other_constraint");

        Assert.False(CreateBookingIdempotencyErrorClassifier.IsIdempotencyKeyUniqueViolation(exception));
    }

    [Fact]
    public void IsIdempotencyKeyUniqueViolation_ShouldReturnFalse_ForNonUniquePostgresError()
    {
        var exception = CreateDbUpdateException(
            PostgresErrorCodes.ForeignKeyViolation,
            CreateBookingIdempotencySchema.PrimaryKeyName);

        Assert.False(CreateBookingIdempotencyErrorClassifier.IsIdempotencyKeyUniqueViolation(exception));
    }

    private static DbUpdateException CreateDbUpdateException(string sqlState, string constraintName)
    {
        var postgresException = new PostgresException(
            "message text",
            "severity",
            "severity",
            sqlState,
            constraintName: constraintName);

        return new DbUpdateException("db update failed", postgresException);
    }
}
