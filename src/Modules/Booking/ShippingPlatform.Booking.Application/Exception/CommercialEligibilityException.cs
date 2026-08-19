using ShippingPlatform.Booking.Application.Port.Out;

namespace ShippingPlatform.Booking.Application.Exception;

public sealed class CommercialEligibilityException(AgreementEligibilityStatus status, string message)
    : System.Exception(message)
{
    public AgreementEligibilityStatus Status { get; } = status;
}