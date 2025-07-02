using AuthService.Domain.DomainEvents;

namespace AuthService.Application.Interfaces;

public interface IDomainEventHandler<in TEvent> 
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);   
}