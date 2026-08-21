using Npgsql;

namespace ShippingPlatform.IntegrationTests.Infrastructure;

internal static class TestDatabaseSafety
{
    private const string AllowedPrefix = "shipping_platform_tests_";

    public static string GetValidatedConnectionString(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(builder.Database) ||
            !builder.Database.StartsWith(AllowedPrefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Integration tests may only operate on shipping_platform_tests_* databases.");
        }

        return connectionString;
    }

    public static bool IsAllowedDatabaseName(string? databaseName)
    {
        return !string.IsNullOrWhiteSpace(databaseName) &&
               databaseName.StartsWith(AllowedPrefix, StringComparison.Ordinal);
    }
}
