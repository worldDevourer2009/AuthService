using AuthService.Application.Interfaces;
using AuthService.Domain.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Application.Services.DomainEvents;

public class DomainDispatcher : IDomainDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var domainEventHandlerType = typeof(IDomainEventHandler<>)
                .MakeGenericType(domainEvent.GetType());

            var handlers = (IEnumerable<object>)_serviceProvider.GetServices(domainEventHandlerType);

            foreach (var handler in handlers)
            {
                dynamic dynHandler = handler;
                dynamic dynEvent = domainEvent;
                await dynHandler.HandleAsync(dynEvent, cancellationToken);
            }
        }
    }
}