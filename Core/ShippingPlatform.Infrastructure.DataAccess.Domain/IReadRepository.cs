using System.Linq.Expressions;

namespace ShippingPlatform.Infrastructure.DataAccess.Domain;

public interface IReadRepository<T>
    where T : class
{
    Task<T?> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    IQueryable<T> Query();
}