using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ShippingPlatform.Domain.Booking.ValueObject;
using ShippingPlatform.Domain.DataAccess;
using ShippingPlatform.Infrastructure.Application;
using BookingAggregate = ShippingPlatform.Domain.Booking.Entity.Booking;

namespace ShippingPlatform.IntegrationTests.Infrastructure;


[Collection(PostgresCollection.Name)]
public class TransactionBehaviorIntegrationTests
{
    private readonly ShippingPlatformApiFactory _factory;

    public TransactionBehaviorIntegrationTests(ShippingPlatformApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TransactionalCommand_ShouldCommit_WhenHandlerCompletesSuccessfully()
    {
        await _factory.ResetDatabaseAsync();
        using var application = CreateApplicationWithTransactionTestHandler();
        using var scope = application.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var customerId = $"CUST-COMMIT-{Guid.NewGuid():N}";
        var command = new TransactionTestCommand(customerId, ShouldFail: false);

        var result = await sender.Send(command);
        Assert.True(result);

        var bookingCount = await _factory.CountBookingsAsync();
        Assert.Equal(1, bookingCount);

        var singleBookingAsync = await _factory.GetSingleBookingAsync();
        Assert.NotNull(singleBookingAsync);
        Assert.Equal(customerId, singleBookingAsync.CustomerId.Value);
    }
    
    [Fact]
    public async Task TransactionalCommand_ShouldRollback_WhenHandlerThrows()
    {
        await _factory.ResetDatabaseAsync();
        using var application = CreateApplicationWithTransactionTestHandler();
        using var scope = application.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var customerId = $"CUST-COMMIT-{Guid.NewGuid():N}";
        var command = new TransactionTestCommand(customerId, ShouldFail: true);

        var throwsAsync = await Assert.ThrowsAsync<InvalidOperationException>(async () => await sender.Send(command));
        Assert.Equal("Forced transaction rollback.", throwsAsync.Message);

        // Important:
        // Query using a fresh DbContext through the factory.
        var countBookingsAsync = await _factory.CountBookingsAsync();
        Assert.Equal(0, countBookingsAsync);
    }
    
    private WebApplicationFactory<Program> CreateApplicationWithTransactionTestHandler()
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddTransient<IRequestHandler<TransactionTestCommand, bool>, TransactionTestCommandHandler>();
            });
        });
    }
    
    internal sealed record TransactionTestCommand(string CustomerId, bool ShouldFail) : ITransactionalCommand<bool>;

    internal sealed class TransactionTestCommandHandler : IRequestHandler<TransactionTestCommand, bool>
    {
        private readonly IWriteUnitOfWork _unitOfWork;

        public TransactionTestCommandHandler(IWriteUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(
            TransactionTestCommand request,
            CancellationToken cancellationToken)
        {
            var booking =
                BookingAggregate.Create(
                    CustomerId.Create(request.CustomerId),
                    AgreementId.Create("AGR-TX-001"),
                    Origin.Create("Bandar Abbas"),
                    Destination.Create("Rotterdam"),
                    VoyageId.Create("VOY-TX-001"));

            await _unitOfWork.Bookings.AddAsync(booking, cancellationToken);

            /*
             * Very important:
             *
             * We intentionally call SaveChanges BEFORE throwing.
             *
             * Without an outer transaction, this INSERT would
             * already be committed to PostgreSQL.
             *
             * TransactionBehavior must keep it inside its
             * transaction.
             */
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (request.ShouldFail)
            {
                throw new InvalidOperationException(
                    "Forced transaction rollback.");
            }

            return true;
        }
    }
}