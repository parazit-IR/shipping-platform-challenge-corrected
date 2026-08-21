using ShippingPlatform.Application.Commercial.Queries.CheckAgreementEligibility;
using ShippingPlatform.Application.Exceptions;
using ShippingPlatform.Domain.Booking.ValueObject;
using ShippingPlatform.Domain.DataAccess;
using ShippingPlatform.Infrastructure.Application;

using BookingAggregate = ShippingPlatform.Domain.Booking.Entity.Booking;

namespace ShippingPlatform.Application.Booking.Commands.Create;

public sealed class CreateBookingCommandHandler
    : ICommandHandler<CreateBookingCommand, CreateBookingResult>
{
    private readonly IAgreementEligibilityChecker _eligibilityChecker;
    private readonly ICreateBookingIdempotencyExecutor _idempotencyExecutor;
    private readonly IWriteUnitOfWork _writeUnitOfWork;

    public CreateBookingCommandHandler(
        IAgreementEligibilityChecker eligibilityChecker,
        ICreateBookingIdempotencyExecutor idempotencyExecutor,
        IWriteUnitOfWork writeUnitOfWork)
    {
        _eligibilityChecker = eligibilityChecker;
        _idempotencyExecutor = idempotencyExecutor;
        _writeUnitOfWork = writeUnitOfWork;
    }

    public async Task<CreateBookingResult> Handle(
        CreateBookingCommand command,
        CancellationToken cancellationToken = default)
    {
        var customerId = CustomerId.Create(command.CustomerId);
        var agreementId = AgreementId.Create(command.AgreementId);
        var origin = Origin.Create(command.Origin);
        var destination = Destination.Create(command.Destination);
        var voyageId = VoyageId.Create(command.VoyageId);

        if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
        {
            return await CreateBookingAsync(
                customerId,
                agreementId,
                origin,
                destination,
                voyageId,
                cancellationToken);
        }

        var fingerprint =
            CreateBookingRequestFingerprint.Compute(
                customerId,
                agreementId,
                origin,
                destination,
                voyageId);

        return await _idempotencyExecutor.ExecuteAsync(
            command.IdempotencyKey.Trim(),
            fingerprint,
            ct => CreateBookingAsync(
                customerId,
                agreementId,
                origin,
                destination,
                voyageId,
                ct),
            cancellationToken);
    }

    private async Task<CreateBookingResult> CreateBookingAsync(
        CustomerId customerId,
        AgreementId agreementId,
        Origin origin,
        Destination destination,
        VoyageId voyageId,
        CancellationToken cancellationToken)
    {
        var eligibility =
            await _eligibilityChecker.CheckAsync(
                customerId.Value,
                agreementId.Value,
                cancellationToken);

        if (!eligibility.IsEligible)
        {
            throw eligibility.Status switch
            {
                CommercialEligibilityStatus.CustomerNotFound =>
                    new CommercialEligibilityException(
                        eligibility.Status,
                        $"Customer '{customerId.Value}' was not found."),

                CommercialEligibilityStatus.AgreementNotFound =>
                    new CommercialEligibilityException(
                        eligibility.Status,
                        $"Agreement '{agreementId.Value}' was not found."),

                CommercialEligibilityStatus.AgreementInactive =>
                    new CommercialEligibilityException(
                        eligibility.Status,
                        $"Agreement '{agreementId.Value}' is inactive."),

                _ =>
                    new CommercialEligibilityException(
                        eligibility.Status,
                        $"Agreement '{agreementId.Value}' is not eligible for booking creation.")
            };
        }

        var booking = BookingAggregate.Create(
            customerId,
            agreementId,
            origin,
            destination,
            voyageId);

        await _writeUnitOfWork.Bookings.AddAsync(
            booking,
            cancellationToken);

        await _writeUnitOfWork.SaveChangesAsync(
            cancellationToken);

        return new CreateBookingResult(
            booking.Id.Value.ToString(),
            booking.Status.ToString());
    }
}
