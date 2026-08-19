using ShippingPlatform.Commercial.Application.Port.In;
using ShippingPlatform.Commercial.Application.Port.Out;
using ShippingPlatform.Commercial.Domain.ValueObject;
using DomainEligibilityStatus = ShippingPlatform.Commercial.Domain.ValueObject.AgreementEligibilityStatus;

namespace ShippingPlatform.Commercial.Application.Service;

public sealed class CheckAgreementEligibilityService(
    ICustomerExistencePort customerExistencePort,
    IAgreementRepository agreementRepository) : ICheckAgreementEligibilityUseCase
{
    public async Task<CheckAgreementEligibilityResult> CheckAsync(CheckAgreementEligibilityCommand command,
        CancellationToken cancellationToken = default)
    {
        var customerId = CustomerId.Create(command.CustomerId);
        var agreementId = AgreementId.Create(command.AgreementId);

        var customerExists = await customerExistencePort.ExistsAsync(customerId, cancellationToken);
        if (!customerExists)
        {
            return new CheckAgreementEligibilityResult(CommercialEligibilityStatus.CustomerNotFound);
        }

        var agreement = await agreementRepository.FindByIdAsync(agreementId, cancellationToken);
        
        if (agreement is null)
        {
            return new CheckAgreementEligibilityResult(CommercialEligibilityStatus.AgreementNotFound);
        }
        
        var agreementEligibilityStatus = agreement.CheckEligibility(customerId);

        var applicationStatus = agreementEligibilityStatus switch
        {
            DomainEligibilityStatus.Eligible => CommercialEligibilityStatus.Eligible,
            DomainEligibilityStatus.AgreementInactive => CommercialEligibilityStatus.AgreementInactive,
            DomainEligibilityStatus.AgreementIneligible => CommercialEligibilityStatus.AgreementIneligible,
            _ => throw new InvalidOperationException($"Unsupported eligibility status: {agreementEligibilityStatus}")
        };
        
        return new CheckAgreementEligibilityResult(applicationStatus);
    }
}