using AuthService.Application.Interfaces;
using AuthService.Domain.DomainEvents.Auth;

namespace AuthService.Application.DomainEvents.DomainEventsHandlers.Auth;

public class UserLoggedInDomainEventHandler : IDomainEventHandler<UserLoggedInDomainEvent>
{
    private readonly IKafkaProducer _kafkaProducer;
    private const string Topic = "user-logged-in";

    public UserLoggedInDomainEventHandler(IKafkaProducer kafkaProducer)
    {
        _kafkaProducer = kafkaProducer;
    }

    public async Task HandleAsync(UserLoggedInDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _kafkaProducer.ProduceAsync(domainEvent, Topic, cancellationToken);
    }
}