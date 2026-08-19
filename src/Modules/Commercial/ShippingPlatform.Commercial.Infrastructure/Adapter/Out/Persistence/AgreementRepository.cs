using Microsoft.EntityFrameworkCore;
using ShippingPlatform.Commercial.Application.Port.Out;
using ShippingPlatform.Commercial.Domain.Entity;
using ShippingPlatform.Commercial.Domain.ValueObject;

namespace ShippingPlatform.Commercial.Infrastructure.Adapter.Out.Persistence;

public sealed class AgreementRepository(CommercialDbContext dbContext) : IAgreementRepository
{
    public async Task<Agreement?> FindByIdAsync(AgreementId agreementId, CancellationToken cancellationToken = default)
    {
        var agreementRecord = await dbContext.Agreements.AsNoTracking().SingleOrDefaultAsync(
            x => x.AgreementId == agreementId.Value, cancellationToken);

        if (agreementRecord is null)
        {
            return null;
        }
        
        return Agreement.Map(
            AgreementId.Create(agreementRecord.AgreementId),
            CustomerId.Create(agreementRecord.CustomerId),
            Enum.Parse<AgreementStatus>(agreementRecord.Status, ignoreCase: true));
    }
}