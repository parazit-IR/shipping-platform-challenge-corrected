using System.Linq.Expressions;
using ShippingPlatform.Application.Booking.Commands.Create;
using ShippingPlatform.Application.Common.Ports;
using ShippingPlatform.Application.Commercial.Queries.CheckAgreementEligibility;
using ShippingPlatform.Domain.Booking;
using ShippingPlatform.Domain.Commercial;
using ShippingPlatform.Domain.Commercial.Entity;
using ShippingPlatform.Domain.Commercial.ValueObject;
using ShippingPlatform.Domain.DataAccess;
using ShippingPlatform.Infrastructure.DataAccess.Domain;

using BookingAggregate = ShippingPlatform.Domain.Booking.Entity.Booking;

namespace ShippingPlatform.Application.Tests.Fakes;

internal sealed class FakeAgreementEligibilityChecker : IAgreementEligibilityChecker
{
    public int CallCount { get; private set; }
    public string? LastCustomerId { get; private set; }
    public string? LastAgreementId { get; private set; }
    public CancellationToken LastCancellationToken { get; private set; }
    public CheckAgreementEligibilityResult ResultToReturn { get; set; } =
        new(CommercialEligibilityStatus.Eligible);

    public Task<CheckAgreementEligibilityResult> CheckAsync(
        string customerId,
        string agreementId,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastCustomerId = customerId;
        LastAgreementId = agreementId;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(ResultToReturn);
    }
}

internal sealed class FakeCreateBookingIdempotencyExecutor : ICreateBookingIdempotencyExecutor
{
    public int CallCount { get; private set; }
    public string? LastIdempotencyKey { get; private set; }
    public string? LastRequestFingerprint { get; private set; }
    public CancellationToken LastCancellationToken { get; private set; }
    public bool ExecuteOperation { get; set; } = true;
    public CreateBookingResult? ResultToReturn { get; set; }
    public Exception? ExceptionToThrow { get; set; }

    public async Task<CreateBookingResult> ExecuteAsync(
        string idempotencyKey,
        string requestFingerprint,
        Func<CancellationToken, Task<CreateBookingResult>> operation,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastIdempotencyKey = idempotencyKey;
        LastRequestFingerprint = requestFingerprint;
        LastCancellationToken = cancellationToken;

        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        if (!ExecuteOperation)
        {
            return ResultToReturn
                   ?? throw new InvalidOperationException("A replay result must be provided when ExecuteOperation is false.");
        }

        return await operation(cancellationToken);
    }
}

internal sealed class FakeBookingWriteRepository : IBookingWriteRepository
{
    public List<BookingAggregate> AddedEntities { get; } = [];

    public Task AddAsync(BookingAggregate entity, CancellationToken cancellationToken = default)
    {
        AddedEntities.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(BookingAggregate entity)
    {
    }

    public void Remove(BookingAggregate entity)
    {
    }
}

internal sealed class FakeAgreementWriteRepository : IAgreementWriteRepository
{
    public Task AddAsync(Agreement entity, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Update(Agreement entity)
    {
    }

    public void Remove(Agreement entity)
    {
    }
}

internal sealed class FakeWriteUnitOfWork : IWriteUnitOfWork
{
    public FakeBookingWriteRepository BookingRepository { get; } = new();
    public int SaveChangesCallCount { get; private set; }
    public CancellationToken LastSaveChangesCancellationToken { get; private set; }

    public IBookingWriteRepository Bookings => BookingRepository;

    public IAgreementWriteRepository Agreements { get; } = new FakeAgreementWriteRepository();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        LastSaveChangesCancellationToken = cancellationToken;
        return Task.FromResult(1);
    }
}

internal sealed class FakeBookingReadRepository : IBookingReadRepository
{
    public Task<BookingAggregate?> FindAsync(
        Expression<Func<BookingAggregate, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<BookingAggregate?>(null);
    }

    public IQueryable<BookingAggregate> Query()
    {
        return Enumerable.Empty<BookingAggregate>().AsQueryable();
    }
}

internal sealed class FakeAgreementReadRepository : IAgreementReadRepository
{
    public Agreement? AgreementToReturn { get; set; }
    public int FindByIdCallCount { get; private set; }
    public AgreementId? LastAgreementId { get; private set; }
    public CancellationToken LastCancellationToken { get; private set; }

    public Task<Agreement?> FindByIdAsync(
        AgreementId agreementId,
        CancellationToken cancellationToken = default)
    {
        FindByIdCallCount++;
        LastAgreementId = agreementId;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(AgreementToReturn);
    }

    public Task<Agreement?> FindAsync(
        Expression<Func<Agreement, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Agreement?>(null);
    }

    public IQueryable<Agreement> Query()
    {
        return AgreementToReturn is null
            ? Enumerable.Empty<Agreement>().AsQueryable()
            : new[] { AgreementToReturn }.AsQueryable();
    }
}

internal sealed class FakeReadUnitOfWork : IReadUnitOfWork
{
    public FakeReadUnitOfWork(FakeAgreementReadRepository agreementRepository)
    {
        AgreementsRepository = agreementRepository;
    }

    public FakeAgreementReadRepository AgreementsRepository { get; }

    public IBookingReadRepository Bookings { get; } = new FakeBookingReadRepository();

    public IAgreementReadRepository Agreements => AgreementsRepository;
}

internal sealed class FakeCustomerExistencePort : ICustomerExistencePort
{
    public bool ExistsResult { get; set; }
    public int CallCount { get; private set; }
    public CustomerId? LastCustomerId { get; private set; }
    public CancellationToken LastCancellationToken { get; private set; }

    public Task<bool> ExistsAsync(CustomerId customerId, CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastCustomerId = customerId;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(ExistsResult);
    }
}
