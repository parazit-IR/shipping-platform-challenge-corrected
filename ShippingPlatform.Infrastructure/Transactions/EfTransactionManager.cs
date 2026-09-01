using ShippingPlatform.Infrastructure.Application.Common.Ports;

namespace ShippingPlatform.Infrastructure.Transactions;

public sealed class EfTransactionManager : ITransactionManager
{
    private readonly Context _context;

    public EfTransactionManager(Context context)
    {
        _context = context;
    }

    public async Task<TResponse> ExecuteAsync<TResponse>(
        Func<Task<TResponse>> operation,
        CancellationToken cancellationToken = default)
    {
        // use current transaction
        if (_context.Database.CurrentTransaction is not null)
        {
            return await operation();
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await operation();
            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}