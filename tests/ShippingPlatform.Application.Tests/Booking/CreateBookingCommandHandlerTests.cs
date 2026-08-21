using ShippingPlatform.Application.Booking.Commands.Create;
using ShippingPlatform.Application.Commercial.Queries.CheckAgreementEligibility;
using ShippingPlatform.Application.Exceptions;
using ShippingPlatform.Application.Tests.Fakes;
using ShippingPlatform.Domain.Booking.Exception;
using ShippingPlatform.Domain.Booking.ValueObject;

namespace ShippingPlatform.Application.Tests.Booking;

public sealed class CreateBookingCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnDraftBooking_WhenInputIsValid()
    {
        var checker = new FakeAgreementEligibilityChecker
        {
            ResultToReturn = new CheckAgreementEligibilityResult(CommercialEligibilityStatus.Eligible)
        };
        var idempotencyExecutor = new FakeCreateBookingIdempotencyExecutor();
        var writeUnitOfWork = new FakeWriteUnitOfWork();
        var handler = new CreateBookingCommandHandler(checker, idempotencyExecutor, writeUnitOfWork);
        var command = new CreateBookingCommand(
            "CUST-001",
            "AGR-001",
            "Bandar Abbas",
            "Rotterdam",
            "VOY-001");
        var cancellationToken = new CancellationTokenSource().Token;

        var result = await handler.Handle(command, cancellationToken);

        Assert.Equal(1, checker.CallCount);
        Assert.Equal("CUST-001", checker.LastCustomerId);
        Assert.Equal("AGR-001", checker.LastAgreementId);
        Assert.Equal(cancellationToken, checker.LastCancellationToken);
        Assert.Equal(0, idempotencyExecutor.CallCount);
        Assert.Single(writeUnitOfWork.BookingRepository.AddedEntities);
        Assert.Equal(1, writeUnitOfWork.SaveChangesCallCount);
        Assert.Equal(cancellationToken, writeUnitOfWork.LastSaveChangesCancellationToken);
        Assert.True(Guid.TryParse(result.BookingId, out _));
        Assert.Equal(BookingStatus.Draft.ToString(), result.Status);

        var persistedBooking = Assert.Single(writeUnitOfWork.BookingRepository.AddedEntities);
        Assert.Equal(BookingStatus.Draft, persistedBooking.Status);
    }

    [Theory]
    [InlineData(CommercialEligibilityStatus.CustomerNotFound, "Customer 'CUST-001' was not found.")]
    [InlineData(CommercialEligibilityStatus.AgreementNotFound, "Agreement 'AGR-001' was not found.")]
    [InlineData(CommercialEligibilityStatus.AgreementInactive, "Agreement 'AGR-001' is inactive.")]
    [InlineData(CommercialEligibilityStatus.AgreementIneligible, "Agreement 'AGR-001' is not eligible for booking creation.")]
    public async Task Handle_ShouldNotPersistBooking_WhenEligibilityFails(
        CommercialEligibilityStatus status,
        string expectedMessage)
    {
        var checker = new FakeAgreementEligibilityChecker
        {
            ResultToReturn = new CheckAgreementEligibilityResult(status)
        };
        var idempotencyExecutor = new FakeCreateBookingIdempotencyExecutor();
        var writeUnitOfWork = new FakeWriteUnitOfWork();
        var handler = new CreateBookingCommandHandler(checker, idempotencyExecutor, writeUnitOfWork);
        var command = new CreateBookingCommand(
            "CUST-001",
            "AGR-001",
            "Bandar Abbas",
            "Rotterdam",
            "VOY-001");

        var exception = await Assert.ThrowsAsync<CommercialEligibilityException>(() => handler.Handle(command));

        Assert.Equal(status, exception.Status);
        Assert.Equal(expectedMessage, exception.Message);
        Assert.Equal(1, checker.CallCount);
        Assert.Equal(0, idempotencyExecutor.CallCount);
        Assert.Empty(writeUnitOfWork.BookingRepository.AddedEntities);
        Assert.Equal(0, writeUnitOfWork.SaveChangesCallCount);
    }

    [Theory]
    [MemberData(nameof(InvalidCommands))]
    public async Task Handle_ShouldNotCallEligibilityChecker_WhenCommandContainsBlankRequiredValues(
        CreateBookingCommand command,
        string expectedMessage)
    {
        var checker = new FakeAgreementEligibilityChecker();
        var idempotencyExecutor = new FakeCreateBookingIdempotencyExecutor();
        var writeUnitOfWork = new FakeWriteUnitOfWork();
        var handler = new CreateBookingCommandHandler(checker, idempotencyExecutor, writeUnitOfWork);

        var exception = await Assert.ThrowsAsync<DomainValidationException>(() => handler.Handle(command));

        Assert.Equal(expectedMessage, exception.Message);
        Assert.Equal(0, checker.CallCount);
        Assert.Equal(0, idempotencyExecutor.CallCount);
        Assert.Empty(writeUnitOfWork.BookingRepository.AddedEntities);
        Assert.Equal(0, writeUnitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ShouldUseIdempotencyExecutor_WhenIdempotencyKeyIsProvided()
    {
        var checker = new FakeAgreementEligibilityChecker
        {
            ResultToReturn = new CheckAgreementEligibilityResult(CommercialEligibilityStatus.Eligible)
        };
        var idempotencyExecutor = new FakeCreateBookingIdempotencyExecutor();
        var writeUnitOfWork = new FakeWriteUnitOfWork();
        var handler = new CreateBookingCommandHandler(checker, idempotencyExecutor, writeUnitOfWork);
        var command = new CreateBookingCommand(
            "CUST-001",
            "AGR-001",
            "Bandar Abbas",
            "Rotterdam",
            "VOY-001",
            "booking-key-001");

        var result = await handler.Handle(command);

        Assert.Equal(1, idempotencyExecutor.CallCount);
        Assert.Equal("booking-key-001", idempotencyExecutor.LastIdempotencyKey);
        Assert.False(string.IsNullOrWhiteSpace(idempotencyExecutor.LastRequestFingerprint));
        Assert.Equal(1, checker.CallCount);
        Assert.Single(writeUnitOfWork.BookingRepository.AddedEntities);
        Assert.Equal(result.BookingId, writeUnitOfWork.BookingRepository.AddedEntities[0].Id.Value.ToString());
    }

    [Fact]
    public async Task Handle_ShouldReturnReplayedResult_WhenIdempotencyExecutorReplaysCompletedRequest()
    {
        var checker = new FakeAgreementEligibilityChecker();
        var idempotencyExecutor = new FakeCreateBookingIdempotencyExecutor
        {
            ExecuteOperation = false,
            ResultToReturn = new CreateBookingResult(Guid.NewGuid().ToString(), "Draft")
        };
        var writeUnitOfWork = new FakeWriteUnitOfWork();
        var handler = new CreateBookingCommandHandler(checker, idempotencyExecutor, writeUnitOfWork);
        var command = new CreateBookingCommand(
            "CUST-001",
            "AGR-001",
            "Bandar Abbas",
            "Rotterdam",
            "VOY-001",
            "booking-key-001");

        var result = await handler.Handle(command);

        Assert.Equal(idempotencyExecutor.ResultToReturn, result);
        Assert.Equal(1, idempotencyExecutor.CallCount);
        Assert.Equal(0, checker.CallCount);
        Assert.Empty(writeUnitOfWork.BookingRepository.AddedEntities);
        Assert.Equal(0, writeUnitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ShouldBubbleIdempotencyConflict_WhenExecutorRejectsDifferentPayload()
    {
        var checker = new FakeAgreementEligibilityChecker();
        var idempotencyExecutor = new FakeCreateBookingIdempotencyExecutor
        {
            ExceptionToThrow = new IdempotencyConflictException("booking-key-001")
        };
        var writeUnitOfWork = new FakeWriteUnitOfWork();
        var handler = new CreateBookingCommandHandler(checker, idempotencyExecutor, writeUnitOfWork);
        var command = new CreateBookingCommand(
            "CUST-001",
            "AGR-001",
            "Bandar Abbas",
            "Rotterdam",
            "VOY-001",
            "booking-key-001");

        var exception = await Assert.ThrowsAsync<IdempotencyConflictException>(() => handler.Handle(command));

        Assert.Equal("booking-key-001", exception.IdempotencyKey);
        Assert.Equal(1, idempotencyExecutor.CallCount);
        Assert.Equal(0, checker.CallCount);
        Assert.Empty(writeUnitOfWork.BookingRepository.AddedEntities);
        Assert.Equal(0, writeUnitOfWork.SaveChangesCallCount);
    }

    public static IEnumerable<object[]> InvalidCommands()
    {
        yield return
        [
            new CreateBookingCommand("", "AGR-001", "Bandar Abbas", "Rotterdam", "VOY-001"),
            "CustomerId is required."
        ];
        yield return
        [
            new CreateBookingCommand(" ", "AGR-001", "Bandar Abbas", "Rotterdam", "VOY-001"),
            "CustomerId is required."
        ];
        yield return
        [
            new CreateBookingCommand("CUST-001", "", "Bandar Abbas", "Rotterdam", "VOY-001"),
            "AgreementId is required."
        ];
        yield return
        [
            new CreateBookingCommand("CUST-001", " ", "Bandar Abbas", "Rotterdam", "VOY-001"),
            "AgreementId is required."
        ];
        yield return
        [
            new CreateBookingCommand("CUST-001", "AGR-001", "", "Rotterdam", "VOY-001"),
            "Origin is required."
        ];
        yield return
        [
            new CreateBookingCommand("CUST-001", "AGR-001", " ", "Rotterdam", "VOY-001"),
            "Origin is required."
        ];
        yield return
        [
            new CreateBookingCommand("CUST-001", "AGR-001", "Bandar Abbas", "", "VOY-001"),
            "Destination is required."
        ];
        yield return
        [
            new CreateBookingCommand("CUST-001", "AGR-001", "Bandar Abbas", " ", "VOY-001"),
            "Destination is required."
        ];
        yield return
        [
            new CreateBookingCommand("CUST-001", "AGR-001", "Bandar Abbas", "Rotterdam", ""),
            "VoyageId is required."
        ];
        yield return
        [
            new CreateBookingCommand("CUST-001", "AGR-001", "Bandar Abbas", "Rotterdam", " "),
            "VoyageId is required."
        ];
    }
}
