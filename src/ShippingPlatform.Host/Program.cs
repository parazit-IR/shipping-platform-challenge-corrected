using ShippingPlatform.Booking.Application.Port.In;
using ShippingPlatform.Booking.Application.Service;
using ShippingPlatform.Booking.Infrastructure.DependencyInjection;
using ShippingPlatform.Booking.Web.DependencyInjection;
using ShippingPlatform.Commercial.Application.Port.In;
using ShippingPlatform.Commercial.Application.Service;
using ShippingPlatform.Commercial.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

// Booking
builder.Services.AddScoped<ICreateBookingUseCase, CreateBookingService>();
builder.Services.AddBookingInfrastructure(builder.Configuration);
builder.Services.AddBookingWebServices();

// Commercial
builder.Services.AddScoped<ICheckAgreementEligibilityUseCase, CheckAgreementEligibilityService>();
builder.Services.AddCommercialInfrastructure(builder.Configuration);


var app = builder.Build();

app.UseExceptionHandler();

app.MapControllers();

app.Run();