using AuthService.Domain.DomainEvents;

namespace AuthService.Application.Interfaces;

public interface IKafkaProducer
{
    Task ProduceAsync(IDomainEvent @event, string topic, CancellationToken cancellationToken = default);
}