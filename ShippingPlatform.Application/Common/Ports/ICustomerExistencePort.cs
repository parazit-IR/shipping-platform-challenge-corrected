using ShippingPlatform.Domain.Commercial.ValueObject;

namespace ShippingPlatform.Application.Common.Ports;

public interface ICustomerExistencePort
{
    Task<bool> ExistsAsync(
        CustomerId customerId,
        CancellationToken cancellationToken = default);
}