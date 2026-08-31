using MediatR;

namespace ShippingPlatform.Infrastructure.Application;

public interface ICommand<out TResponse>: IRequest<TResponse>
{
    
}