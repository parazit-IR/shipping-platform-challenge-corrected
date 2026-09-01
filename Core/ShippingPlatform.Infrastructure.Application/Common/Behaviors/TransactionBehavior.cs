using MediatR;
using ShippingPlatform.Infrastructure.Application;
using ShippingPlatform.Infrastructure.Application.Common.Ports;

public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITransactionManager _transactionManager;

    public TransactionBehavior(ITransactionManager transactionManager)
    {
        _transactionManager = transactionManager;
    }


    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // is command and need transaction
        if (request is not ITransactionalCommand<TResponse>)
        {
            return await next(cancellationToken);
        }

        return await _transactionManager.ExecuteAsync(
            async () => await next(cancellationToken), cancellationToken);
    }
}