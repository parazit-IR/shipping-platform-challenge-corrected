using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShippingPlatform.Booking.Infrastructure.Adapter.Out.Persistence;

public sealed class BookingDbContextFactory: IDesignTimeDbContextFactory<BookingDbContext>
{
    public BookingDbContext CreateDbContext(string[] args)
    {
        //todo - not safe -  move to environment
        var connectionString =
            Environment.GetEnvironmentVariable(
                "BOOKING_DB_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=shipping_platform;Username=postgres;Password=postgres";

        var optionsBuilder =
            new DbContextOptionsBuilder<BookingDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    "booking"));

        return new BookingDbContext(
            optionsBuilder.Options);
    }
}