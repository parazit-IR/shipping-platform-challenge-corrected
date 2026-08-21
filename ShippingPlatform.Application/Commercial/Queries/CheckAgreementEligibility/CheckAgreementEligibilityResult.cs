namespace ShippingPlatform.Application.Commercial.Queries.CheckAgreementEligibility;

public sealed record CheckAgreementEligibilityResult(CommercialEligibilityStatus Status)
{
    public bool IsEligible => Status == CommercialEligibilityStatus.Eligible;
}