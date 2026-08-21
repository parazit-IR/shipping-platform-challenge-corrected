using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShippingPlatform.Infrastructure.DataAccess.Domain;

namespace ShippingPlatform.Infrastructure.DataAccess;

public class ReadRepository<T> : IReadRepository<T>
    where T : class
{
    protected readonly DbContext Context;
    protected readonly DbSet<T> DbSet;

    public ReadRepository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual Task<T?> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual IQueryable<T> Query()
    {
        return DbSet.AsNoTracking();
    }
}