using ShippingPlatform.Domain.Commercial.Entity;
using ShippingPlatform.Domain.Commercial.ValueObject;
using ShippingPlatform.Infrastructure.DataAccess.Domain;

namespace ShippingPlatform.Domain.Commercial;

public interface IAgreementReadRepository : IReadRepository<Agreement>
{
    Task<Agreement?> FindByIdAsync(AgreementId agreementId, CancellationToken cancellationToken = default);
}