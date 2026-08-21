using Xunit;

namespace ShippingPlatform.IntegrationTests.Infrastructure;

[CollectionDefinition("PostgreSql", DisableParallelization = true)]
public sealed class PostgresCollection : ICollectionFixture<ShippingPlatformApiFactory>
{
    public const string Name = "PostgreSql";
}
