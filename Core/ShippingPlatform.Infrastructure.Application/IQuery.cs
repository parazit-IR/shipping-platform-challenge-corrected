using MediatR;

namespace ShippingPlatform.Infrastructure.Application;

public interface IQuery<out TResponse>: IRequest<TResponse>
{
    
}