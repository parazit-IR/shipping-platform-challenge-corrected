using Microsoft.EntityFrameworkCore;
using ShippingPlatform.Application.Common.Ports;
using ShippingPlatform.Domain.Commercial.ValueObject;

namespace ShippingPlatform.Infrastructure.Commercial;

public sealed class CustomerExistenceAdapter : ICustomerExistencePort
{
    private readonly Context _context;

    public CustomerExistenceAdapter(Context context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(
        CustomerId customerId,
        CancellationToken cancellationToken = default)
    {
        return _context.Customers.AnyAsync(
            x => x.CustomerId == customerId,
            cancellationToken);
    }
}