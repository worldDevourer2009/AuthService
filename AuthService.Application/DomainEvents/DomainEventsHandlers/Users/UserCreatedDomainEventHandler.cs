using AuthService.Application.Interfaces;
using AuthService.Domain.DomainEvents.Users;

namespace AuthService.Application.DomainEvents.DomainEventsHandlers.Users;

public class UserCreatedDomainEventHandler : IDomainEventHandler<UserCreatedDomainEvent>
{
    private readonly IKafkaProducer _kafkaProducer;
    private const string Topic = "user-created";

    public UserCreatedDomainEventHandler(IKafkaProducer kafkaProducer)
    {
        _kafkaProducer = kafkaProducer;
    }

    public async Task HandleAsync(UserCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _kafkaProducer.ProduceAsync(domainEvent, Topic, cancellationToken);       
    }
}