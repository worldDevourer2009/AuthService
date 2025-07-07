using AuthService.Domain.DomainEvents;

namespace AuthService.Application.Interfaces;

public interface IDomainDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}