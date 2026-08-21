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
        Assert.True(await _factory.TableExistsAsync("commercial", "customers"));
        Assert.True(await _factory.TableExistsAsync("commercial", "agreements"));
        Assert.True(await _factory.AgreementCustomerForeignKeyExistsAsync());

        await using var connection = new NpgsqlConnection(_factory.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "insert into commercial.agreements (agreement_id, customer_id, status) values ('AGR-001', 'CUST-404', 'Active');",
            connection);

        var exception = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync());

        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, exception.SqlState);
    }
}
