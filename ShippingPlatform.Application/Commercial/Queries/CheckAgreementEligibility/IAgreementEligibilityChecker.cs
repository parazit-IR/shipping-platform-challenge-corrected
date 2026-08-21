namespace ShippingPlatform.Application.Commercial.Queries.CheckAgreementEligibility;

public interface IAgreementEligibilityChecker
{
    Task<CheckAgreementEligibilityResult> CheckAsync(
        string customerId,
        string agreementId,
        CancellationToken cancellationToken = default);
}