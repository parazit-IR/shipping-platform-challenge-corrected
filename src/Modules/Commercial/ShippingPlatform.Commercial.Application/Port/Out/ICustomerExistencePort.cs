using ShippingPlatform.Commercial.Domain.ValueObject;

namespace ShippingPlatform.Commercial.Application.Port.Out;

public interface ICustomerExistencePort
{
    Task<bool> ExistsAsync(CustomerId customerId, CancellationToken cancellationToken = default);
}