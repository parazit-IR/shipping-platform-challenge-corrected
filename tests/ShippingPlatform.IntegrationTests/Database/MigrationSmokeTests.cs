using Npgsql;
using ShippingPlatform.IntegrationTests.Infrastructure;

namespace ShippingPlatform.IntegrationTests.Database;

[Collection(PostgresCollection.Name)]
public sealed class MigrationSmokeTests
{
    private readonly ShippingPlatformApiFactory _factory;

    public MigrationSmokeTests(ShippingPlatformApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Migrate_ShouldCreateExpectedTablesAndAgreementCustomerForeignKey()
    {
        await _factory.ResetDatabaseAsync();

        Assert.True(await _factory.TableExistsAsync("booking", "bookings"));
        Assert.True(await _factory.TableExistsAsync("booking", "create_booking_idempotency"));
        Assert.True(await _factory.TableExistsAsync("commercial", "customers"));
        Assert.True(await _factory.TableExistsAsync("commercial", "agreements"));
        Assert.True(await _factory.AgreementCustomerForeignKeyExistsAsync());
        Assert.True(await _factory.HasCreateBookingIdempotencyPrimaryKeyConstraintAsync());
        Assert.False(await _factory.HasCreateBookingIdempotencyRedundantUniqueIndexAsync());

        await using var connection = new NpgsqlConnection(_factory.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "insert into commercial.agreements (agreement_id, customer_id, status) values ('AGR-001', 'CUST-404', 'Active');",
            connection);

        var exception = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync());

        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, exception.SqlState);
    }

    [Fact]
    public async Task Migrate_ShouldEnforceUniqueCreateBookingIdempotencyKey()
    {
        await _factory.ResetDatabaseAsync();

        await using var connection = new NpgsqlConnection(_factory.ConnectionString);
        await connection.OpenAsync();

        await using (var firstCommand = new NpgsqlCommand(
                         """
                         insert into booking.create_booking_idempotency
                             (idempotency_key, request_fingerprint, state, created_at)
                         values
                             ('booking-key-001', 'ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789', 'Pending', now());
                         """,
                         connection))
        {
            await firstCommand.ExecuteNonQueryAsync();
        }

        await using var secondCommand = new NpgsqlCommand(
            """
            insert into booking.create_booking_idempotency
                (idempotency_key, request_fingerprint, state, created_at)
            values
                ('booking-key-001', 'FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF', 'Pending', now());
            """,
            connection);

        var exception = await Assert.ThrowsAsync<PostgresException>(() => secondCommand.ExecuteNonQueryAsync());

        Assert.Equal(PostgresErrorCodes.UniqueViolation, exception.SqlState);
        Assert.Equal(
            ShippingPlatform.Infrastructure.Booking.CreateBookingIdempotencySchema.PrimaryKeyName,
            exception.ConstraintName);
    }
}
