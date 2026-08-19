namespace ShippingPlatform.Commercial.Application.Port.In;

public sealed record CheckAgreementEligibilityResult(CommercialEligibilityStatus Status)
{
    public bool IsEligible => Status == CommercialEligibilityStatus.Eligible;
}