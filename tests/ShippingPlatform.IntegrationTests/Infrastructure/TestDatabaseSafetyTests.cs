namespace ShippingPlatform.IntegrationTests.Infrastructure;

public sealed class TestDatabaseSafetyTests
{
    [Theory]
    [InlineData("shipping_platform")]
    [InlineData("shipping_platform_dev")]
    [InlineData("postgres")]
    [InlineData("")]
    public void IsAllowedDatabaseName_ShouldReturnFalse_ForNonTestDatabases(string databaseName)
    {
        Assert.False(TestDatabaseSafety.IsAllowedDatabaseName(databaseName));
    }

    [Fact]
    public void IsAllowedDatabaseName_ShouldReturnTrue_ForTestDatabasePrefix()
    {
        Assert.True(TestDatabaseSafety.IsAllowedDatabaseName("shipping_platform_tests_123"));
    }
}
