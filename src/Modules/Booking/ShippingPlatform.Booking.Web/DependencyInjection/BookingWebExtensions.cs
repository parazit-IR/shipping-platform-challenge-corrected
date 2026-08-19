using Microsoft.Extensions.DependencyInjection;
using ShippingPlatform.Booking.Web.Controller;
using ShippingPlatform.Booking.Web.ExceptionHandling;

namespace ShippingPlatform.Booking.Web.DependencyInjection;

public static class BookingWebExtensions
{
    public static IServiceCollection AddBookingWebServices(this IServiceCollection services)
    {
        services.AddExceptionHandler<BookingExceptionHandler>();
        services.AddControllers().AddApplicationPart(typeof(BookingsController).Assembly);
        return services;
    }
    
}