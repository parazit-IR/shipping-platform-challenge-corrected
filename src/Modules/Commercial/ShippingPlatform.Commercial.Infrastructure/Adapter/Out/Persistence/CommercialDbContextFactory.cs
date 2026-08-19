using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShippingPlatform.Commercial.Infrastructure.Adapter.Out.Persistence;

public sealed class CommercialDbContextFactory: IDesignTimeDbContextFactory<CommercialDbContext>
{
    public CommercialDbContext CreateDbContext(string[] args)
    {
        //todo - not safe -  move to environment
        var connectionString =
            Environment.GetEnvironmentVariable(
                "COMMERCIAL_DB_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=shipping_platform;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<CommercialDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    "commercial"));

        return new CommercialDbContext(
            optionsBuilder.Options);
    }
}