using ShippingPlatform.Booking.Application.Exception;
using ShippingPlatform.Booking.Application.Port.In;
using ShippingPlatform.Booking.Application.Port.Out;
using ShippingPlatform.Booking.Domain.ValueObject;
using BookingAggregate = ShippingPlatform.Booking.Domain.Entity.Booking;


namespace ShippingPlatform.Booking.Application.Service;

public class CreateBookingService(
    IAgreementEligibilityPort agreementEligibilityPort,
    IBookingRepository bookingRepository): ICreateBookingUseCase
{
    public async Task<CreateBookingResult> CreateAsync(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        var eligibilityResult = await agreementEligibilityPort.CheckAsync(
            command.CustomerId,
            command.AgreementId,
            command.Origin,
            command.Destination,
            command.VoyageId,
            cancellationToken);

        if (!eligibilityResult.IsEligible)
        {
            throw eligibilityResult.Status switch
            {
                AgreementEligibilityStatus.CustomerNotFound =>
                    new CommercialEligibilityException(
                        eligibilityResult.Status, $"Customer '{command.CustomerId}' was not found."),

                AgreementEligibilityStatus.AgreementNotFound =>
                    new CommercialEligibilityException(
                        eligibilityResult.Status, $"Agreement '{command.AgreementId}' was not found."),

                AgreementEligibilityStatus.AgreementInactive =>
                    new CommercialEligibilityException(
                        eligibilityResult.Status, $"Agreement '{command.AgreementId}' is inactive."),

                _ =>
                    new CommercialEligibilityException(
                        eligibilityResult.Status, $"Agreement '{command.AgreementId}' is not eligible for booking creation.")
            };
        }
        
        var booking = BookingAggregate.Create(
            CustomerId.Create(command.CustomerId),
            AgreementId.Create(command.AgreementId),
            Origin.Create(command.Origin),
            Destination.Create(command.Destination),
            VoyageId.Create(command.VoyageId));

        await bookingRepository.AddAsync(booking, cancellationToken);
        await bookingRepository.SaveChangesAsync(cancellationToken);

        return new CreateBookingResult(booking.Id.Value.ToString(), booking.Status.ToString());
    }
}