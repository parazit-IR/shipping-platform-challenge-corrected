using ShippingPlatform.Booking.Application.Port.In;
using ShippingPlatform.Booking.Application.Service;
using ShippingPlatform.Booking.Infrastructure.DependencyInjection;
using ShippingPlatform.Booking.Web.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

// Booking Application
builder.Services.AddScoped<ICreateBookingUseCase, CreateBookingService>();

// Booking Infrastructure
builder.Services.AddBookingInfrastructure(builder.Configuration);

// Booking Web
builder.Services.AddBookingWebServices();

var app = builder.Build();

app.UseExceptionHandler();

app.MapControllers();

app.Run();