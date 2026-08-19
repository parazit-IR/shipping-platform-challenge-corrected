using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShippingPlatform.Commercial.Infrastructure.Adapter.Out.Persistence;

public sealed class CommercialDbContextFactory: IDesignTimeDbContextFactory<CommercialDbContext>
{
    public CommercialDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "COMMERCIAL_DB_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "COMMERCIAL_DB_CONNECTION_STRING is not configured.");

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