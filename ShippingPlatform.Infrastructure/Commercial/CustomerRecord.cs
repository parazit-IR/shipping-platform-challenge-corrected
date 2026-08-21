using ShippingPlatform.Domain.Commercial.ValueObject;

namespace ShippingPlatform.Infrastructure.Commercial;

internal sealed class CustomerRecord
{
    public CustomerId CustomerId { get; private set; } = null!;

    private CustomerRecord()
    {
    }
}