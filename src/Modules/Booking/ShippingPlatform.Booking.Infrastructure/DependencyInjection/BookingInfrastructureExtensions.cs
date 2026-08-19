using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShippingPlatform.Booking.Application.Port.Out;
using ShippingPlatform.Booking.Infrastructure.Adapter.Out.Commercial;
using ShippingPlatform.Booking.Infrastructure.Adapter.Out.Persistence;

namespace ShippingPlatform.Booking.Infrastructure.DependencyInjection;

public static class BookingInfrastructureExtensions
{
    public static IServiceCollection AddBookingInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ShippingPlatform") ??
                               throw new InvalidOperationException(
                                   "Connection string 'ShippingPlatform' is not configured.");
        
        services.AddDbContext<BookingDbContext>(options => options.UseNpgsql(
            connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "booking")));
        
        services.AddScoped<IBookingRepository, BookingRepository>();

        services.AddScoped<IAgreementEligibilityPort, CommercialAgreementEligibilityAdapter>();

        return services;
    }
}