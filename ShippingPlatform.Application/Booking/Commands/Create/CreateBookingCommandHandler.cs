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
    private readonly IWriteUnitOfWork _writeUnitOfWork;

    public CreateBookingCommandHandler(
        IAgreementEligibilityChecker eligibilityChecker,
        IWriteUnitOfWork writeUnitOfWork)
    {
        _eligibilityChecker = eligibilityChecker;
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
                        $"Customer '{command.CustomerId}' was not found."),

                CommercialEligibilityStatus.AgreementNotFound =>
                    new CommercialEligibilityException(
                        eligibility.Status,
                        $"Agreement '{command.AgreementId}' was not found."),

                CommercialEligibilityStatus.AgreementInactive =>
                    new CommercialEligibilityException(
                        eligibility.Status,
                        $"Agreement '{command.AgreementId}' is inactive."),

                _ =>
                    new CommercialEligibilityException(
                        eligibility.Status,
                        $"Agreement '{command.AgreementId}' is not eligible for booking creation.")
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
