using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using ShippingPlatform.Api.Booking.Contract;
using ShippingPlatform.IntegrationTests.Infrastructure;

namespace ShippingPlatform.IntegrationTests.Booking;

[Collection(PostgresCollection.Name)]
public sealed class CreateBookingApiTests
{
    private readonly ShippingPlatformApiFactory _factory;

    public CreateBookingApiTests(ShippingPlatformApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostBooking_ShouldReturn201AndPersistBooking_WhenInputIsValid()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedCustomerAsync("CUST-001");
        await _factory.SeedAgreementAsync("AGR-001", "CUST-001", "Active");

        var response = await _factory.Client.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingRequest
            {
                CustomerId = "CUST-001",
                AgreementId = "AGR-001",
                Origin = "Bandar Abbas",
                Destination = "Rotterdam",
                VoyageId = "VOY-001"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CreateBookingResponse>();

        Assert.NotNull(body);
        Assert.True(Guid.TryParse(body.BookingId, out _));
        Assert.Equal("Draft", body.Status);
        Assert.Equal(1, await _factory.CountBookingsAsync());

        var booking = await _factory.GetSingleBookingAsync();

        Assert.NotNull(booking);
        Assert.Equal("CUST-001", booking.CustomerId.Value);
        Assert.Equal("AGR-001", booking.AgreementId.Value);
        Assert.Equal("Bandar Abbas", booking.Origin.Value);
        Assert.Equal("Rotterdam", booking.Destination.Value);
        Assert.Equal("VOY-001", booking.VoyageId.Value);
    }

    [Fact]
    public async Task PostBooking_ShouldReturn404AndNotPersist_WhenCustomerDoesNotExist()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _factory.Client.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingRequest
            {
                CustomerId = "CUST-001",
                AgreementId = "AGR-001",
                Origin = "Bandar Abbas",
                Destination = "Rotterdam",
                VoyageId = "VOY-001"
            });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal("Customer not found", problem.Title);
        Assert.Equal("Customer 'CUST-001' was not found.", problem.Detail);
        Assert.Equal(0, await _factory.CountBookingsAsync());
    }

    [Fact]
    public async Task PostBooking_ShouldReturn404AndNotPersist_WhenAgreementDoesNotExist()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedCustomerAsync("CUST-001");

        var response = await _factory.Client.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingRequest
            {
                CustomerId = "CUST-001",
                AgreementId = "AGR-001",
                Origin = "Bandar Abbas",
                Destination = "Rotterdam",
                VoyageId = "VOY-001"
            });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal("Agreement not found", problem.Title);
        Assert.Equal("Agreement 'AGR-001' was not found.", problem.Detail);
        Assert.Equal(0, await _factory.CountBookingsAsync());
    }

    [Fact]
    public async Task PostBooking_ShouldReturn422AndNotPersist_WhenAgreementIsInactive()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedCustomerAsync("CUST-001");
        await _factory.SeedAgreementAsync("AGR-001", "CUST-001", "Inactive");

        var response = await _factory.Client.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingRequest
            {
                CustomerId = "CUST-001",
                AgreementId = "AGR-001",
                Origin = "Bandar Abbas",
                Destination = "Rotterdam",
                VoyageId = "VOY-001"
            });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(422, problem.Status);
        Assert.Equal("Agreement inactive", problem.Title);
        Assert.Equal("Agreement 'AGR-001' is inactive.", problem.Detail);
        Assert.Equal(0, await _factory.CountBookingsAsync());
    }

    [Fact]
    public async Task PostBooking_ShouldReturn422AndNotPersist_WhenAgreementBelongsToDifferentCustomer()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedCustomerAsync("CUST-001");
        await _factory.SeedCustomerAsync("CUST-999");
        await _factory.SeedAgreementAsync("AGR-001", "CUST-999", "Active");

        var response = await _factory.Client.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingRequest
            {
                CustomerId = "CUST-001",
                AgreementId = "AGR-001",
                Origin = "Bandar Abbas",
                Destination = "Rotterdam",
                VoyageId = "VOY-001"
            });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(422, problem.Status);
        Assert.Equal("Agreement not eligible", problem.Title);
        Assert.Equal("Agreement 'AGR-001' is not eligible for booking creation.", problem.Detail);
        Assert.Equal(0, await _factory.CountBookingsAsync());
    }

    [Fact]
    public async Task PostBooking_ShouldReturn400AndNotPersist_WhenCustomerIdIsBlank()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _factory.Client.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingRequest
            {
                CustomerId = " ",
                AgreementId = "AGR-001",
                Origin = "Bandar Abbas",
                Destination = "Rotterdam",
                VoyageId = "VOY-001"
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("CustomerId", problem.Errors.Keys);
        Assert.NotEmpty(problem.Errors["CustomerId"]);
        Assert.Equal(0, await _factory.CountBookingsAsync());
    }
}
