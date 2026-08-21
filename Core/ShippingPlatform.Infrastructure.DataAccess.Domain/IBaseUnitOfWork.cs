namespace ShippingPlatform.Infrastructure.DataAccess.Domain;

public interface IBaseUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}