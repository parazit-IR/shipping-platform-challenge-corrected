using ShippingPlatform.Application.Commercial.Queries.CheckAgreementEligibility;

namespace ShippingPlatform.Application.Exceptions;

public sealed class CommercialEligibilityException : Exception
{
    public CommercialEligibilityStatus Status { get; }

    public CommercialEligibilityException(CommercialEligibilityStatus status, string message) : base(message)
    {
        Status = status;
    }
}