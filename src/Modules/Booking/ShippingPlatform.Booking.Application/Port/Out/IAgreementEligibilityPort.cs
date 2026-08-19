namespace ShippingPlatform.Booking.Application.Port.Out;

public interface IAgreementEligibilityPort
{
    Task<AgreementEligibilityResult> CheckAsync(
        string customerId,
        string agreementId,
        string origin,
        string destination,
        string voyageId,
        CancellationToken cancellationToken);
}