using Microsoft.EntityFrameworkCore;
using ShippingPlatform.Commercial.Application.Port.Out;
using ShippingPlatform.Commercial.Domain.ValueObject;

namespace ShippingPlatform.Commercial.Infrastructure.Adapter.Out.Persistence;


//because is not aggregate (instead of CustomerRepository)
public sealed class CustomerExistenceAdapter(CommercialDbContext dbContext): ICustomerExistencePort
{
    public Task<bool> ExistsAsync(CustomerId customerId, CancellationToken cancellationToken = default)
    {
        return dbContext.Customers.AnyAsync(
            x => x.CustomerId == customerId.Value, cancellationToken);
    }
}