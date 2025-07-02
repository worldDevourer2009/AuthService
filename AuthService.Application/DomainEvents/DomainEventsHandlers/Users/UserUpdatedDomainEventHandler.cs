using AuthService.Application.Interfaces;
using AuthService.Domain.DomainEvents.Users;

namespace AuthService.Application.DomainEvents.DomainEventsHandlers.Users;

public class UserUpdatedDomainEventHandler : IDomainEventHandler<UserUpdatedDomainEvent>
{
    private readonly IKafkaProducer _kafkaProducer;
    private const string Topic = "user-updated";

    public UserUpdatedDomainEventHandler(IKafkaProducer kafkaProducer)
    {
        _kafkaProducer = kafkaProducer;
    }

    public async Task HandleAsync(UserUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _kafkaProducer.ProduceAsync(domainEvent, Topic, cancellationToken);
    }   
}