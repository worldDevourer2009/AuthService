using AuthService.Application.Interfaces;
using AuthService.Domain.DomainEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Services.DomainEvents;

public class DomainDispatcher : IDomainDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainDispatcher> _logger;

    public DomainDispatcher(IServiceProvider serviceProvider, ILogger<DomainDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        try
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
                    _logger.LogInformation("Dispatching domain event {DomainEvent}", domainEvent.GetType().Name);
                    await dynHandler.HandleAsync(dynEvent, cancellationToken);
                    _logger.LogInformation("Domain event {DomainEvent} dispatched", domainEvent.GetType().Name);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while dispatching domain events");
        }
    }
}