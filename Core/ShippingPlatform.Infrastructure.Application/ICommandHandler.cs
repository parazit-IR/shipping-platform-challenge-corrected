using MediatR;

namespace ShippingPlatform.Infrastructure.Application;

public interface ICommandHandler<in TCommand, TResponse>:  IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(
        TCommand command,
        CancellationToken cancellationToken = default);
}