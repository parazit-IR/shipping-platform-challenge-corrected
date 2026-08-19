using ShippingPlatform.Booking.Application.Port.Out;

namespace ShippingPlatform.Booking.Infrastructure.Adapter.Out.Commercial;

public class AlwaysEligibleAgreementAdapter : IAgreementEligibilityPort
{
    public Task<AgreementEligibilityResult> CheckAsync(string customerId, string agreementId, string origin,
        string destination, string voyageId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new AgreementEligibilityResult(AgreementEligibilityStatus.Eligible));
    }
}