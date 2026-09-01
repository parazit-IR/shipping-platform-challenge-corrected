namespace ShippingPlatform.Infrastructure.Application.Common.Ports;

public interface ITransactionManager
{
    Task<TResponse> ExecuteAsync<TResponse>(
        Func<Task<TResponse>> operation,
        CancellationToken cancellationToken = default);
}