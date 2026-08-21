using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using ShippingPlatform.Infrastructure;

namespace ShippingPlatform.IntegrationTests.Infrastructure;

public sealed class ShippingPlatformApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly DockerPostgresDatabase _database = new();
    private readonly string? _externalConnectionString;

    public HttpClient Client { get; private set; } = null!;

    public string ConnectionString => _externalConnectionString ?? _database.ConnectionString;

    public ShippingPlatformApiFactory()
    {
    }

    private ShippingPlatformApiFactory(string connectionString)
    {
        _externalConnectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:ShippingPlatform"] = ConnectionString
                });
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<Context>();
            services.RemoveAll<DbContextOptions<Context>>();

            services.AddDbContext<Context>(options => options.UseNpgsql(
                GetValidatedConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public")));
        });
    }

    public async Task InitializeAsync()
    {
        if (_externalConnectionString is null)
        {
            await _database.StartAsync();
        }

        Client = CreateClient();

        if (_externalConnectionString is null)
        {
            await ResetDatabaseAsync();
        }
    }

    public new async Task DisposeAsync()
    {
        if (_externalConnectionString is null)
        {
            await _database.DisposeAsync();
        }

        Dispose();
    }

    public async Task<ShippingPlatformApiFactory> CreateSiblingFactoryAsync()
    {
        var sibling = new ShippingPlatformApiFactory(ConnectionString);
        await sibling.InitializeAsync();
        return sibling;
    }

    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Context>();
        TestDatabaseSafety.GetValidatedConnectionString(
            context.Database.GetConnectionString()
            ?? throw new InvalidOperationException("The test Context does not have a connection string."));

        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }

    public async Task SeedCustomerAsync(string customerId)
    {
        await using var connection = new NpgsqlConnection(GetValidatedConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "insert into commercial.customers (customer_id) values (@customerId);",
            connection);
        command.Parameters.AddWithValue("customerId", customerId);
        await command.ExecuteNonQueryAsync();
    }

    public async Task SeedAgreementAsync(string agreementId, string customerId, string status)
    {
        await using var connection = new NpgsqlConnection(GetValidatedConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "insert into commercial.agreements (agreement_id, customer_id, status) values (@agreementId, @customerId, @status);",
            connection);
        command.Parameters.AddWithValue("agreementId", agreementId);
        command.Parameters.AddWithValue("customerId", customerId);
        command.Parameters.AddWithValue("status", status);
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAgreementStatusAsync(string agreementId, string status)
    {
        await using var connection = new NpgsqlConnection(GetValidatedConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            update commercial.agreements
            set status = @status
            where agreement_id = @agreementId;
            """,
            connection);
        command.Parameters.AddWithValue("agreementId", agreementId);
        command.Parameters.AddWithValue("status", status);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> CountBookingsAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Context>();
        return await context.Bookings.CountAsync();
    }

    public async Task<ShippingPlatform.Domain.Booking.Entity.Booking?> GetSingleBookingAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Context>();
        return await context.Bookings.SingleOrDefaultAsync();
    }

    public async Task<bool> TableExistsAsync(string schema, string table)
    {
        await using var connection = new NpgsqlConnection(GetValidatedConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            select exists (
                select 1
                from information_schema.tables
                where table_schema = @schema
                  and table_name = @table);
            """,
            connection);
        command.Parameters.AddWithValue("schema", schema);
        command.Parameters.AddWithValue("table", table);
        return (bool)(await command.ExecuteScalarAsync() ?? false);
    }

    public async Task<bool> AgreementCustomerForeignKeyExistsAsync()
    {
        await using var connection = new NpgsqlConnection(GetValidatedConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            select exists (
                select 1
                from information_schema.table_constraints tc
                join information_schema.key_column_usage kcu
                  on tc.constraint_name = kcu.constraint_name
                 and tc.table_schema = kcu.table_schema
                where tc.constraint_type = 'FOREIGN KEY'
                  and tc.table_schema = 'commercial'
                  and tc.table_name = 'agreements'
                  and kcu.column_name = 'customer_id');
            """,
            connection);
        return (bool)(await command.ExecuteScalarAsync() ?? false);
    }

    public async Task<int> CountCreateBookingIdempotencyRecordsAsync(string? idempotencyKey = null)
    {
        await using var connection = new NpgsqlConnection(GetValidatedConnectionString());
        await connection.OpenAsync();
        await using var command = idempotencyKey is null
            ? new NpgsqlCommand(
                "select count(*) from booking.create_booking_idempotency;",
                connection)
            : new NpgsqlCommand(
                """
                select count(*)
                from booking.create_booking_idempotency
                where idempotency_key = @idempotencyKey;
                """,
                connection);

        if (idempotencyKey is not null)
        {
            command.Parameters.AddWithValue("idempotencyKey", idempotencyKey);
        }

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task<CreateBookingIdempotencyRecordDto?> GetCreateBookingIdempotencyRecordAsync(string idempotencyKey)
    {
        await using var connection = new NpgsqlConnection(GetValidatedConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            select idempotency_key, request_fingerprint, state, booking_id, booking_status
            from booking.create_booking_idempotency
            where idempotency_key = @idempotencyKey;
            """,
            connection);
        command.Parameters.AddWithValue("idempotencyKey", idempotencyKey);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new CreateBookingIdempotencyRecordDto(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetGuid(3),
            reader.IsDBNull(4) ? null : reader.GetString(4));
    }

    public async Task<bool> HasCreateBookingIdempotencyKeyUniqueConstraintAsync()
    {
        await using var connection = new NpgsqlConnection(GetValidatedConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            select exists (
                select 1
                from pg_indexes
                where schemaname = 'booking'
                  and tablename = 'create_booking_idempotency'
                  and indexdef ilike '%unique%'
                  and indexdef ilike '%(idempotency_key)%');
            """,
            connection);
        return (bool)(await command.ExecuteScalarAsync() ?? false);
    }

    public async Task<bool> HasCreateBookingIdempotencyPrimaryKeyConstraintAsync()
    {
        await using var connection = new NpgsqlConnection(GetValidatedConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            select exists (
                select 1
                from information_schema.table_constraints
                where table_schema = 'booking'
                  and table_name = 'create_booking_idempotency'
                  and constraint_type = 'PRIMARY KEY'
                  and constraint_name = @constraintName);
            """,
            connection);
        command.Parameters.AddWithValue(
            "constraintName",
            ShippingPlatform.Infrastructure.Booking.CreateBookingIdempotencySchema.PrimaryKeyName);
        return (bool)(await command.ExecuteScalarAsync() ?? false);
    }

    public async Task<bool> HasCreateBookingIdempotencyRedundantUniqueIndexAsync()
    {
        await using var connection = new NpgsqlConnection(GetValidatedConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            select exists (
                select 1
                from pg_indexes
                where schemaname = 'booking'
                  and tablename = 'create_booking_idempotency'
                  and indexname = 'IX_create_booking_idempotency_idempotency_key');
            """,
            connection);
        return (bool)(await command.ExecuteScalarAsync() ?? false);
    }

    private string GetValidatedConnectionString()
    {
        return TestDatabaseSafety.GetValidatedConnectionString(ConnectionString);
    }
}
