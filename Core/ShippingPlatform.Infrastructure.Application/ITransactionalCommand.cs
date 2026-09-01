namespace ShippingPlatform.Infrastructure.Application;

public interface ITransactionalCommand<out TResponse>: ICommand<TResponse>
{
    
}