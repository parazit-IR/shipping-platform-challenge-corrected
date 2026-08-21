using Microsoft.EntityFrameworkCore;
using ShippingPlatform.Infrastructure.DataAccess.Domain;

namespace ShippingPlatform.Infrastructure.DataAccess;

public class BaseUnitOfWork : IBaseUnitOfWork
{
    protected readonly DbContext Context;

    public BaseUnitOfWork(DbContext context)
    {
        Context = context;
    }

    public virtual Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}