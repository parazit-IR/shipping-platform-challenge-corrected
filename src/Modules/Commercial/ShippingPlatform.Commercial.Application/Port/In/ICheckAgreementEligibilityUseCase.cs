namespace ShippingPlatform.Commercial.Application.Port.In;

public interface ICheckAgreementEligibilityUseCase
{
    Task<CheckAgreementEligibilityResult> CheckAsync(CheckAgreementEligibilityCommand command,
        CancellationToken cancellationToken = default);
}