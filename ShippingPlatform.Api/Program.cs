using ShippingPlatform.Api.ExceptionHandling;
using ShippingPlatform.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddControllers();

builder.Services.AddShippingPlatform(
    builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

app.MapControllers();

app.Run();

public partial class Program
{
}
