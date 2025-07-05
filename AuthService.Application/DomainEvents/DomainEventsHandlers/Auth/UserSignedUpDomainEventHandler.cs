using AuthService.Application.Interfaces;
using AuthService.Domain.DomainEvents.Auth;

namespace AuthService.Application.DomainEvents.DomainEventsHandlers.Auth;

public class UserSignedUpDomainEventHandler : IDomainEventHandler<UserSignedUpDomainEvent>
{
    private readonly IKafkaProducer _kafkaProducer;
    private const string Topic = "user-signed-up";

    public UserSignedUpDomainEventHandler(IKafkaProducer kafkaProducer)
    {
        _kafkaProducer = kafkaProducer;
    }

    public async Task HandleAsync(UserSignedUpDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _kafkaProducer.ProduceAsync(domainEvent, Topic, cancellationToken);
    }
}