using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using ShippingPlatform.Api.Booking.Contract;
using ShippingPlatform.IntegrationTests.Infrastructure;

namespace ShippingPlatform.IntegrationTests.Booking;

[Collection(PostgresCollection.Name)]
public sealed class CreateBookingApiTests
{
    private const string IdempotencyHeader = "Idempotency-Key";

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

        var response = await SendCreateBookingAsync(
            _factory.Client,
            "CUST-001",
            "AGR-001",
            "Bandar Abbas",
            "Rotterdam",
            "VOY-001");

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

        var response = await SendCreateBookingAsync(
            _factory.Client,
            "CUST-001",
            "AGR-001",
            "Bandar Abbas",
            "Rotterdam",
            "VOY-001");

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

        var response = await SendCreateBookingAsync(
            _factory.Client,
            "CUST-001",
            "AGR-001",
            "Bandar Abbas",
            "Rotterdam",
            "VOY-001");

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

        var response = await SendCreateBookingAsync(
            _factory.Client,
            "CUST-001",
            "AGR-001",
            "Bandar Abbas",
            "Rotterdam",
            "VOY-001");

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

        var response = await SendCreateBookingAsync(
            _factory.Client,
            "CUST-001",
            "AGR-001",
            "Bandar Abbas",
            "Rotterdam",
            "VOY-001");

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

        var response = await SendCreateBookingAsync(
            _factory.Client,
            " ",
            "AGR-001",
            "Bandar Abbas",
            "Rotterdam",
            "VOY-001");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("CustomerId", problem.Errors.Keys);
        Assert.NotEmpty(problem.Errors["CustomerId"]);
        Assert.Equal(0, await _factory.CountBookingsAsync());
    }

    [Fact]
    public async Task PostBooking_ShouldReturn400_WhenIdempotencyKeyHeaderIsBlank()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedCustomerAsync("CUST-001");
        await _factory.SeedAgreementAsync("AGR-001", "CUST-001", "Active");

        var response = await SendCreateBookingAsync(
            _factory.Client,
            "CUST-001",
            "AGR-001",
            "Bandar Abbas",
            "Rotterdam",
            "VOY-001",
            " ");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("Idempotency-Key", problem.Errors.Keys);
        Assert.NotEmpty(problem.Errors["Idempotency-Key"]);
        Assert.Equal(0, await _factory.CountBookingsAsync());
    }

    [Fact]
    public async Task PostBooking_ShouldReturn201AndPersistIdempotencyRecord_WhenFirstIdempotentRequestSucceeds()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedCustomerAsync("CUST-001");
        await _factory.SeedAgreementAsync("AGR-001", "CUST-001", "Active");

        var response = await SendCreateBookingAsync(
            _factory.Client,
            "CUST-001",
            "AGR-001",
            "Shanghai",
            "Rotterdam",
            "VOY-001",
            "booking-key-001");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CreateBookingResponse>();
        var idempotencyRecord = await _factory.GetCreateBookingIdempotencyRecordAsync("booking-key-001");

        Assert.NotNull(body);
        Assert.Equal(1, await _factory.CountBookingsAsync());
        Assert.Equal(1, await _factory.CountCreateBookingIdempotencyRecordsAsync("booking-key-001"));
        Assert.NotNull(idempotencyRecord);
        Assert.Equal(body.BookingId, idempotencyRecord.BookingId!.Value.ToString());
        Assert.Equal("Draft", idempotencyRecord.BookingStatus);
        Assert.Equal("Completed", idempotencyRecord.State);
    }

    [Fact]
    public async Task PostBooking_ShouldReplayStableResult_WhenSameIdempotencyKeyAndPayloadAreRepeated()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedCustomerAsync("CUST-001");
        await _factory.SeedAgreementAsync("AGR-001", "CUST-001", "Active");

        var firstResponse = await SendCreateBookingAsync(
            _factory.Client,
            "CUST-001",
            "AGR-001",
            "Shanghai",
            "Rotterdam",
            "VOY-001",
            "booking-key-001");
        var secondResponse = await SendCreateBookingAsync(
            _factory.Client,
            "CUST-001",
            "AGR-001",
            "Shanghai",
            "Rotterdam",
            "VOY-001",
            "booking-key-001");

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);

        var firstBody = await firstResponse.Content.ReadFromJsonAsync<CreateBookingResponse>();
        var secondBody = await secondResponse.Content.ReadFromJsonAsync<CreateBookingResponse>();

        Assert.NotNull(firstBody);
        Assert.NotNull(secondBody);
        Assert.Equal(firstBody.BookingId, secondBody.BookingId);
        Assert.Equal(firstBody.Status, secondBody.Status);
        Assert.Equal(1, await _factory.CountBookingsAsync());
        Assert.Equal(1, await _factory.CountCreateBookingIdempotencyRecordsAsync("booking-key-001"));
    }

    [Fact]
    public async Task PostBooking_ShouldReturn409AndKeepOriginalBooking_WhenSameIdempotencyKeyIsUsedWithDifferentPayload()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedCustomerAsync("CUST-001");
        await _factory.SeedAgreementAsync("AGR-001", "CUST-001", "Active");

        var firstResponse = await SendCreateBookingAsync(
            _factory.Client,
            "CUST-001",
            "AGR-001",
            "Shanghai",
            "Rotterdam",
            "VOY-001",
            "booking-key-001");
        var conflictResponse = await SendCreateBookingAsync(
            _factory.Client,
            "CUST-001",
            "AGR-001",
            "Shanghai",
            "Hamburg",
            "VOY-001",
            "booking-key-001");

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);

        var conflict = await conflictResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        var booking = await _factory.GetSingleBookingAsync();

        Assert.NotNull(conflict);
        Assert.Equal(409, conflict.Status);
        Assert.Equal("Idempotency key conflict", conflict.Title);
        Assert.Equal(
            "Idempotency key 'booking-key-001' cannot be reused with a different request payload.",
            conflict.Detail);
        Assert.Equal(1, await _factory.CountBookingsAsync());
        Assert.Equal(1, await _factory.CountCreateBookingIdempotencyRecordsAsync("booking-key-001"));
        Assert.NotNull(booking);
        Assert.Equal("Shanghai", booking.Origin.Value);
        Assert.Equal("Rotterdam", booking.Destination.Value);
    }

    [Fact]
    public async Task PostBooking_ShouldNotPersistIdempotencyRecord_WhenIdempotentRequestFailsBusinessValidation()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedCustomerAsync("CUST-001");
        await _factory.SeedAgreementAsync("AGR-001", "CUST-001", "Inactive");

        var response = await SendCreateBookingAsync(
            _factory.Client,
            "CUST-001",
            "AGR-001",
            "Shanghai",
            "Rotterdam",
            "VOY-001",
            "failed-key-001");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(0, await _factory.CountBookingsAsync());
        Assert.Equal(0, await _factory.CountCreateBookingIdempotencyRecordsAsync("failed-key-001"));
    }

    [Fact]
    public async Task PostBooking_ShouldAllowRetryWithSameKey_AfterBusinessFailureIsFixed()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedCustomerAsync("CUST-001");
        await _factory.SeedAgreementAsync("AGR-001", "CUST-001", "Inactive");

        var failedResponse = await SendCreateBookingAsync(
            _factory.Client,
            "CUST-001",
            "AGR-001",
            "Shanghai",
            "Rotterdam",
            "VOY-001",
            "retry-after-failure-001");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, failedResponse.StatusCode);
        Assert.Equal(0, await _factory.CountBookingsAsync());
        Assert.Equal(0, await _factory.CountCreateBookingIdempotencyRecordsAsync("retry-after-failure-001"));

        await _factory.UpdateAgreementStatusAsync("AGR-001", "Active");

        var successResponse = await SendCreateBookingAsync(
            _factory.Client,
            "CUST-001",
            "AGR-001",
            "Shanghai",
            "Rotterdam",
            "VOY-001",
            "retry-after-failure-001");

        Assert.Equal(HttpStatusCode.Created, successResponse.StatusCode);

        var body = await successResponse.Content.ReadFromJsonAsync<CreateBookingResponse>();
        var idempotencyRecord = await _factory.GetCreateBookingIdempotencyRecordAsync("retry-after-failure-001");

        Assert.NotNull(body);
        Assert.Equal(1, await _factory.CountBookingsAsync());
        Assert.Equal(1, await _factory.CountCreateBookingIdempotencyRecordsAsync("retry-after-failure-001"));
        Assert.NotNull(idempotencyRecord);
        Assert.Equal("Completed", idempotencyRecord.State);
        Assert.Equal(body.BookingId, idempotencyRecord.BookingId!.Value.ToString());
    }

    [Fact]
    public async Task PostBooking_ShouldCreateExactlyOneBookingAcrossConcurrentSamePayloadRequests()
    {
        const int requestCount = 10;

        for (var iteration = 0; iteration < 5; iteration++)
        {
            await _factory.ResetDatabaseAsync();
            await _factory.SeedCustomerAsync("CUST-001");
            await _factory.SeedAgreementAsync("AGR-001", "CUST-001", "Active");

            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var tasks = Enumerable.Range(0, requestCount)
                .Select(_ => SendConcurrentCreateBookingAsync(
                    _factory.Client,
                    gate.Task,
                    "CUST-001",
                    "AGR-001",
                    "Shanghai",
                    "Rotterdam",
                    "VOY-001",
                    "booking-key-001"))
                .ToArray();

            gate.SetResult();

            var responses = await Task.WhenAll(tasks);

            Assert.All(
                responses,
                response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));

            var bodies = await Task.WhenAll(
                responses.Select(response => response.Content.ReadFromJsonAsync<CreateBookingResponse>()));

            Assert.DoesNotContain(
                responses,
                response => (int)response.StatusCode >= 500);
            Assert.All(bodies, body => Assert.NotNull(body));
            Assert.Single(bodies.Select(body => body!.BookingId).Distinct());
            Assert.Equal(1, await _factory.CountBookingsAsync());
            Assert.Equal(1, await _factory.CountCreateBookingIdempotencyRecordsAsync("booking-key-001"));
        }
    }

    [Fact]
    public async Task PostBooking_ShouldAllowOneWinnerAndOneConflict_ForConcurrentDifferentPayloadRequests()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedCustomerAsync("CUST-001");
        await _factory.SeedAgreementAsync("AGR-001", "CUST-001", "Active");

        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var firstTask = SendConcurrentCreateBookingAsync(
            _factory.Client,
            gate.Task,
            "CUST-001",
            "AGR-001",
            "Shanghai",
            "Rotterdam",
            "VOY-001",
            "booking-key-001");
        var secondTask = SendConcurrentCreateBookingAsync(
            _factory.Client,
            gate.Task,
            "CUST-001",
            "AGR-001",
            "Shanghai",
            "Hamburg",
            "VOY-001",
            "booking-key-001");

        gate.SetResult();

        var responses = await Task.WhenAll(firstTask, secondTask);

        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        Assert.DoesNotContain(responses, response => (int)response.StatusCode >= 500);
        Assert.Equal(1, await _factory.CountBookingsAsync());
        Assert.Equal(1, await _factory.CountCreateBookingIdempotencyRecordsAsync("booking-key-001"));
    }

    [Fact]
    public async Task PostBooking_ShouldReplayPersistedResult_AfterApplicationHostRestart()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedCustomerAsync("CUST-001");
        await _factory.SeedAgreementAsync("AGR-001", "CUST-001", "Active");

        var firstResponse = await SendCreateBookingAsync(
            _factory.Client,
            "CUST-001",
            "AGR-001",
            "Shanghai",
            "Rotterdam",
            "VOY-001",
            "booking-key-001");
        var firstBody = await firstResponse.Content.ReadFromJsonAsync<CreateBookingResponse>();

        await using var restartedFactory = await _factory.CreateSiblingFactoryAsync();

        var replayResponse = await SendCreateBookingAsync(
            restartedFactory.Client,
            "CUST-001",
            "AGR-001",
            "Shanghai",
            "Rotterdam",
            "VOY-001",
            "booking-key-001");
        var replayBody = await replayResponse.Content.ReadFromJsonAsync<CreateBookingResponse>();

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, replayResponse.StatusCode);
        Assert.NotNull(firstBody);
        Assert.NotNull(replayBody);
        Assert.Equal(firstBody.BookingId, replayBody.BookingId);
        Assert.Equal(1, await _factory.CountBookingsAsync());
        Assert.Equal(1, await _factory.CountCreateBookingIdempotencyRecordsAsync("booking-key-001"));
    }

    private static async Task<HttpResponseMessage> SendConcurrentCreateBookingAsync(
        HttpClient client,
        Task gate,
        string customerId,
        string agreementId,
        string origin,
        string destination,
        string voyageId,
        string? idempotencyKey = null)
    {
        await gate;
        return await SendCreateBookingAsync(
            client,
            customerId,
            agreementId,
            origin,
            destination,
            voyageId,
            idempotencyKey);
    }

    private static Task<HttpResponseMessage> SendCreateBookingAsync(
        HttpClient client,
        string customerId,
        string agreementId,
        string origin,
        string destination,
        string voyageId,
        string? idempotencyKey = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/bookings")
        {
            Content = JsonContent.Create(
                new CreateBookingRequest
                {
                    CustomerId = customerId,
                    AgreementId = agreementId,
                    Origin = origin,
                    Destination = destination,
                    VoyageId = voyageId
                })
        };

        if (idempotencyKey is not null)
        {
            request.Headers.Add(IdempotencyHeader, idempotencyKey);
        }

        return client.SendAsync(request);
    }
}
