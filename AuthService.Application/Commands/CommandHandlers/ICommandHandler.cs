using MediatR;

namespace AuthService.Application.Commands.CommandHandlers;

public interface ICommandHandler<in TCommand, TResponse> : 
    IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse> 
    where TResponse : notnull
{
}