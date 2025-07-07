using AuthService.Application.Interfaces;
using AuthService.Domain.DomainEvents.Auth;

namespace AuthService.Application.DomainEvents.DomainEventsHandlers.Auth;

public class UserLoggedOutDomainEventHandler : IDomainEventHandler<UserLoggedOutDomainEvent>
{
    private readonly IKafkaProducer _kafkaProducer;
    private const string Topic = "user-logged-out";

    public UserLoggedOutDomainEventHandler(IKafkaProducer kafkaProducer)
    {
        _kafkaProducer = kafkaProducer;
    }

    public async Task HandleAsync(UserLoggedOutDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _kafkaProducer.ProduceAsync(domainEvent, Topic, cancellationToken);
    }
}