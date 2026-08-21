using Microsoft.EntityFrameworkCore;
using ShippingPlatform.Infrastructure.DataAccess.Domain;

namespace ShippingPlatform.Infrastructure.DataAccess;

public class BaseReadUnitOfWork : IBaseReadUnitOfWork
{
    protected readonly DbContext Context;

    public BaseReadUnitOfWork(DbContext context)
    {
        Context = context;
    }
}