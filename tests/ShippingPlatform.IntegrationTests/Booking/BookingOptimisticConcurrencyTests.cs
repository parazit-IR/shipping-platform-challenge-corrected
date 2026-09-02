using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShippingPlatform.Domain.Booking.ValueObject;
using ShippingPlatform.Infrastructure;
using ShippingPlatform.IntegrationTests.Infrastructure;

using BookingAggregate = ShippingPlatform.Domain.Booking.Entity.Booking;

namespace ShippingPlatform.IntegrationTests.Booking;

// Real-PostgreSQL coverage for database-backed optimistic concurrency control on the Booking
// aggregate (explicit `version` column, EF Core concurrency token). EF Core InMemory does not
// enforce concurrency tokens, so these tests exercise the actual Npgsql provider end to end.
[Collection(PostgresCollection.Name)]
public sealed class BookingOptimisticConcurrencyTests
{
    private readonly ShippingPlatformApiFactory _factory;

    public BookingOptimisticConcurrencyTests(ShippingPlatformApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ReloadedBooking_ShouldHaveVersionOne_WhenNewlyCreated()
    {
        await _factory.ResetDatabaseAsync();

        var bookingId = await CreateBookingAsync();

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Context>();

        var reloaded = await context.Bookings.SingleAsync(b => b.Id == bookingId);

        Assert.Equal(1, reloaded.Version);
    }

    [Fact]
    public async Task Version_ShouldIncrementToTwoAndPersistChange_WhenBookingIsUpdatedAndSaved()
    {
        await _factory.ResetDatabaseAsync();
        var bookingId = await CreateBookingAsync();

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<Context>();
            var booking = await context.Bookings.SingleAsync(b => b.Id == bookingId);

            booking.AddContainerRequest(ContainerRequest.Create("Dry", "40ft", 1, 1000m));
            booking.RequestQuotation();

            await context.SaveChangesAsync();
        }

        await using var verifyScope = _factory.Services.CreateAsyncScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<Context>();
        var reloaded = await verifyContext.Bookings.SingleAsync(b => b.Id == bookingId);

        Assert.Equal(2, reloaded.Version);
        Assert.Equal(BookingStatus.QuotationRequested, reloaded.Status);
    }

    [Fact]
    public async Task SecondContextSave_ShouldThrowDbUpdateConcurrencyExceptionAndNotOverwrite_WhenBookingWasConcurrentlyModified()
    {
        await _factory.ResetDatabaseAsync();
        var bookingId = await CreateBookingAsync();

        // Two genuinely independent DbContext instances/scopes, each loading the same row.
        await using var scopeA = _factory.Services.CreateAsyncScope();
        var contextA = scopeA.ServiceProvider.GetRequiredService<Context>();

        await using var scopeB = _factory.Services.CreateAsyncScope();
        var contextB = scopeB.ServiceProvider.GetRequiredService<Context>();

        var bookingA = await contextA.Bookings.SingleAsync(b => b.Id == bookingId);
        var bookingB = await contextB.Bookings.SingleAsync(b => b.Id == bookingId);

        Assert.Equal(1, bookingA.Version);
        Assert.Equal(1, bookingB.Version);

        // Context A performs a real domain mutation.
        bookingA.AddContainerRequest(ContainerRequest.Create("Dry", "40ft", 1, 1000m));
        bookingA.RequestQuotation();

        // Context B independently mutates its own (stale) tracked copy to a different target
        // value, simulating a second, genuinely concurrent writer. This goes through EF's
        // change-tracking API directly rather than a new domain business method, since the
        // aggregate only exposes one real status transition today -- the point under test is
        // the persistence-layer concurrency check, not domain transition rules.
        contextB.Entry(bookingB).Property(nameof(BookingAggregate.Status)).CurrentValue =
            BookingStatus.Cancelled;

        await contextA.SaveChangesAsync();

        await using (var afterAScope = _factory.Services.CreateAsyncScope())
        {
            var afterAContext = afterAScope.ServiceProvider.GetRequiredService<Context>();
            var afterA = await afterAContext.Bookings.SingleAsync(b => b.Id == bookingId);

            Assert.Equal(2, afterA.Version);
            Assert.Equal(BookingStatus.QuotationRequested, afterA.Status);
        }

        // The stale save must fail because the generated UPDATE ... WHERE version = 1 now
        // affects zero rows (the database row is already at version 2). EF Core must raise
        // this itself -- it is not thrown manually anywhere in this test or in the production
        // code under test.
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => contextB.SaveChangesAsync());

        await using var finalScope = _factory.Services.CreateAsyncScope();
        var finalContext = finalScope.ServiceProvider.GetRequiredService<Context>();
        var final = await finalContext.Bookings.SingleAsync(b => b.Id == bookingId);

        Assert.Equal(2, final.Version);
        Assert.Equal(BookingStatus.QuotationRequested, final.Status);
        Assert.NotEqual(BookingStatus.Cancelled, final.Status);
    }

    private async Task<BookingId> CreateBookingAsync()
    {
        var booking = BookingAggregate.Create(
            CustomerId.Create("CUST-CONCURRENCY-001"),
            AgreementId.Create("AGR-CONCURRENCY-001"),
            Origin.Create("Bandar Abbas"),
            Destination.Create("Rotterdam"),
            VoyageId.Create("VOY-CONCURRENCY-001"));

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Context>();

        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        return booking.Id;
    }
}
