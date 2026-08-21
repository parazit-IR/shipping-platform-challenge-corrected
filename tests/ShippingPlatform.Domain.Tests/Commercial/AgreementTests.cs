using ShippingPlatform.Domain.Commercial.Entity;
using ShippingPlatform.Domain.Commercial.ValueObject;

namespace ShippingPlatform.Domain.Tests.Commercial;

public sealed class AgreementTests
{
    [Fact]
    public void CheckEligibility_ShouldReturnEligible_WhenAgreementIsActiveAndBelongsToCustomer()
    {
        var agreement = Agreement.Create(
            AgreementId.Create("AGR-001"),
            CustomerId.Create("CUST-001"),
            AgreementStatus.Active);

        var result = agreement.CheckEligibility(CustomerId.Create("CUST-001"));

        Assert.Equal(AgreementEligibilityStatus.Eligible, result);
    }

    [Fact]
    public void CheckEligibility_ShouldReturnAgreementInactive_WhenAgreementIsInactive()
    {
        var agreement = Agreement.Create(
            AgreementId.Create("AGR-001"),
            CustomerId.Create("CUST-001"),
            AgreementStatus.Inactive);

        var result = agreement.CheckEligibility(CustomerId.Create("CUST-001"));

        Assert.Equal(AgreementEligibilityStatus.AgreementInactive, result);
    }

    [Fact]
    public void CheckEligibility_ShouldReturnAgreementIneligible_WhenAgreementBelongsToDifferentCustomer()
    {
        var agreement = Agreement.Create(
            AgreementId.Create("AGR-001"),
            CustomerId.Create("CUST-002"),
            AgreementStatus.Active);

        var result = agreement.CheckEligibility(CustomerId.Create("CUST-001"));

        Assert.Equal(AgreementEligibilityStatus.AgreementIneligible, result);
    }
}
