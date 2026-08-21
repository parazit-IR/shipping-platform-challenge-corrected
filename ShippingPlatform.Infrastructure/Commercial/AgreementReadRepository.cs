using Microsoft.EntityFrameworkCore;
using ShippingPlatform.Domain.Commercial;
using ShippingPlatform.Domain.Commercial.ValueObject;
using ShippingPlatform.Infrastructure.DataAccess;
using AgreementAggregate = ShippingPlatform.Domain.Commercial.Entity.Agreement;

namespace ShippingPlatform.Infrastructure.Commercial;

public sealed class AgreementReadRepository : ReadRepository<AgreementAggregate>, IAgreementReadRepository
{
    public AgreementReadRepository(Context context) : base(context)
    {
    }

    public Task<AgreementAggregate?> FindByIdAsync(
        AgreementId agreementId,
        CancellationToken cancellationToken = default)
    {
        return DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == agreementId,
                cancellationToken);
    }
}