using AuthService.Application.Interfaces;
using AuthService.Domain.DomainEvents.Users;

namespace AuthService.Application.DomainEvents.DomainEventsHandlers.Users;

public class UserDeletedDomainEventHandler : IDomainEventHandler<UserDeletedDomainEvent>
{
    private readonly IKafkaProducer _kafkaProducer;
    private const string Topic = "user-deleted";

    public UserDeletedDomainEventHandler(IKafkaProducer kafkaProducer)
    {
        _kafkaProducer = kafkaProducer;
    }

    public async Task HandleAsync(UserDeletedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _kafkaProducer.ProduceAsync(domainEvent, Topic, cancellationToken);
    }
}