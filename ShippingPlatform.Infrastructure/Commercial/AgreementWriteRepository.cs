using ShippingPlatform.Domain.Commercial;
using ShippingPlatform.Infrastructure.DataAccess;
using AgreementAggregate = ShippingPlatform.Domain.Commercial.Entity.Agreement;

namespace ShippingPlatform.Infrastructure.Commercial;

public sealed class AgreementWriteRepository : WriteRepository<AgreementAggregate>, IAgreementWriteRepository
{
    public AgreementWriteRepository(Context context) : base(context)
    {
    }
}