using Microsoft.EntityFrameworkCore;
using ShippingPlatform.Infrastructure.DataAccess.Domain;

namespace ShippingPlatform.Infrastructure.DataAccess;

public class WriteRepository<T> : IWriteRepository<T>
    where T : class
{
    protected readonly DbContext Context;
    protected readonly DbSet<T> DbSet;

    public WriteRepository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task AddAsync(
        T entity,
        CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        DbSet.Remove(entity);
    }
}