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
        var eligibility =
            await _eligibilityChecker.CheckAsync(
                command.CustomerId,
                command.AgreementId,
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
            CustomerId.Create(command.CustomerId),
            AgreementId.Create(command.AgreementId),
            Origin.Create(command.Origin),
            Destination.Create(command.Destination),
            VoyageId.Create(command.VoyageId));

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