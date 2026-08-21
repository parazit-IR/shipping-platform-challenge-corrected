using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShippingPlatform.Application.Booking.Commands.Create;
using ShippingPlatform.Application.Common.Ports;
using ShippingPlatform.Application.Commercial.Queries.CheckAgreementEligibility;
using ShippingPlatform.Domain.DataAccess;
using ShippingPlatform.Infrastructure;
using ShippingPlatform.Infrastructure.Application;
using ShippingPlatform.Infrastructure.Commercial;

namespace ShippingPlatform.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddShippingPlatform(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("ShippingPlatform")
            ?? throw new InvalidOperationException("Connection string 'ShippingPlatform' is not configured.");
        
        services.AddDbContext<Context>(
            options => options.UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public")));

        services.AddScoped<IReadUnitOfWork, ReadUnitOfWork>();
        services.AddScoped<IWriteUnitOfWork, WriteUnitOfWork>();

        services.AddScoped<
            ICustomerExistencePort,
            CustomerExistenceAdapter>();

        services.AddScoped<
            CheckAgreementEligibilityQueryHandler>();

        services.AddScoped<
            IQueryHandler<
                CheckAgreementEligibilityQuery,
                CheckAgreementEligibilityResult>>(
            sp => sp.GetRequiredService<
                CheckAgreementEligibilityQueryHandler>());

        services.AddScoped<
            IAgreementEligibilityChecker>(
            sp => sp.GetRequiredService<
                CheckAgreementEligibilityQueryHandler>());

        services.AddScoped<
            ICommandHandler<
                CreateBookingCommand,
                CreateBookingResult>,
            CreateBookingCommandHandler>();

        return services;
    }
}