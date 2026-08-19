using ShippingPlatform.Booking.Application.Port.Out;
using ShippingPlatform.Commercial.Application.Port.In;

namespace ShippingPlatform.Booking.Infrastructure.Adapter.Out.Commercial;

public sealed class CommercialAgreementEligibilityAdapter(ICheckAgreementEligibilityUseCase useCase)
    : IAgreementEligibilityPort
{
    public async Task<AgreementEligibilityResult> CheckAsync(
        string customerId,
        string agreementId,
        string origin,
        string destination,
        string voyageId,
        CancellationToken cancellationToken = default)
    {
        var result = await useCase.CheckAsync(
            new CheckAgreementEligibilityCommand(customerId, agreementId, origin, destination, voyageId),
            cancellationToken);

        var status = result.Status switch
        {
            CommercialEligibilityStatus.Eligible => AgreementEligibilityStatus.Eligible,
            CommercialEligibilityStatus.CustomerNotFound => AgreementEligibilityStatus.CustomerNotFound,
            CommercialEligibilityStatus.AgreementNotFound => AgreementEligibilityStatus.AgreementNotFound,
            CommercialEligibilityStatus.AgreementInactive => AgreementEligibilityStatus.AgreementInactive,
            CommercialEligibilityStatus.AgreementIneligible => AgreementEligibilityStatus.AgreementIneligible,
            _ => throw new InvalidOperationException($"Unsupported Commercial eligibility status: {result.Status}")
        };

        return new AgreementEligibilityResult(status);
    }
}