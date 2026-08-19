using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShippingPlatform.Commercial.Application.Port.Out;
using ShippingPlatform.Commercial.Infrastructure.Adapter.Out.Persistence;

namespace ShippingPlatform.Commercial.Infrastructure.DependencyInjection;

public static class CommercialInfrastructureExtensions
{
    public static IServiceCollection AddCommercialInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("ShippingPlatform")
            ?? throw new InvalidOperationException("Connection string 'ShippingPlatform' is not configured.");

        services.AddDbContext<CommercialDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        "commercial")));

        services.AddScoped<IAgreementRepository, AgreementRepository>();
        services.AddScoped<ICustomerExistencePort, CustomerExistenceAdapter>();

        return services;
    }
}