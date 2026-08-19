namespace ShippingPlatform.Booking.Application.Port.Out;

public sealed record AgreementEligibilityResult(AgreementEligibilityStatus Status)
{
    public bool IsEligible => Status == AgreementEligibilityStatus.Eligible;
}