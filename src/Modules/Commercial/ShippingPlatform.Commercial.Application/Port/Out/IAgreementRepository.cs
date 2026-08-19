using ShippingPlatform.Commercial.Domain.Entity;
using ShippingPlatform.Commercial.Domain.ValueObject;

namespace ShippingPlatform.Commercial.Application.Port.Out;

public interface IAgreementRepository
{
    Task<Agreement?> FindByIdAsync(AgreementId agreementId, CancellationToken cancellationToken = default);
}