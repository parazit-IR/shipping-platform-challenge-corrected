using ShippingPlatform.Commercial.Domain.ValueObject;
using ShippingPlatform.SharedKernel;

namespace ShippingPlatform.Commercial.Domain.Entity;

public sealed class Agreement : AggregateRoot
{
    public AgreementId Id { get; }
    public CustomerId CustomerId { get; }
    public AgreementStatus Status { get; }


    private Agreement(AgreementId id, CustomerId customerId, AgreementStatus status)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
        Status = status;
    }

    public static Agreement Create(AgreementId id, CustomerId customerId, AgreementStatus status)
    {
        return new Agreement(
            id,
            customerId,
            status);
    }

    public AgreementEligibilityStatus CheckEligibility(CustomerId requestedCustomerId)
    {
        if (Status != AgreementStatus.Active)
        {
            return AgreementEligibilityStatus.AgreementInactive;
        }

        if (CustomerId != requestedCustomerId)
        {
            return AgreementEligibilityStatus.AgreementIneligible;
        }

        return AgreementEligibilityStatus.Eligible;
    }
}