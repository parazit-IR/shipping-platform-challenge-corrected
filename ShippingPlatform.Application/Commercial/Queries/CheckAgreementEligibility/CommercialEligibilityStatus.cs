namespace ShippingPlatform.Application.Commercial.Queries.CheckAgreementEligibility;

public enum CommercialEligibilityStatus
{
    Eligible = 1,
    CustomerNotFound = 2,
    AgreementNotFound = 3,
    AgreementInactive = 4,
    AgreementIneligible = 5
}