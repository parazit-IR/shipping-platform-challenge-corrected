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

    public HttpClient Client { get; private set; } = null!;

    public string ConnectionString => _database.ConnectionString;

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
                GetSafeConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public")));
        });
    }

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        Client = CreateClient();
        await ResetDatabaseAsync();
    }

    public new async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        Dispose();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Context>();
        EnsureSafeDatabaseName(
            context.Database.GetConnectionString()
            ?? throw new InvalidOperationException("The test Context does not have a connection string."));

        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }

    public async Task SeedCustomerAsync(string customerId)
    {
        await using var connection = new NpgsqlConnection(GetSafeConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "insert into commercial.customers (customer_id) values (@customerId);",
            connection);
        command.Parameters.AddWithValue("customerId", customerId);
        await command.ExecuteNonQueryAsync();
    }

    public async Task SeedAgreementAsync(string agreementId, string customerId, string status)
    {
        await using var connection = new NpgsqlConnection(GetSafeConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "insert into commercial.agreements (agreement_id, customer_id, status) values (@agreementId, @customerId, @status);",
            connection);
        command.Parameters.AddWithValue("agreementId", agreementId);
        command.Parameters.AddWithValue("customerId", customerId);
        command.Parameters.AddWithValue("status", status);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> CountBookingsAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Context>();
        return await context.Bookings.CountAsync();
    }

    public async Task<Domain.Booking.Entity.Booking?> GetSingleBookingAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Context>();
        return await context.Bookings.SingleOrDefaultAsync();
    }

    public async Task<bool> TableExistsAsync(string schema, string table)
    {
        await using var connection = new NpgsqlConnection(GetSafeConnectionString());
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
        await using var connection = new NpgsqlConnection(GetSafeConnectionString());
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

    private string GetSafeConnectionString()
    {
        EnsureSafeDatabaseName(ConnectionString);
        return ConnectionString;
    }

    private static void EnsureSafeDatabaseName(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        if (string.Equals(builder.Database, "shipping_platform", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Integration tests are configured to use the development database 'shipping_platform'. Aborting to protect non-test data.");
        }
    }
}