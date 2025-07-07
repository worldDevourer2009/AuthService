namespace AuthService.Domain.DomainEvents;

public interface IDomainEvent
{
    Guid? Id { get; }
    DateTime? CreatedAt { get; }
    string EventType { get; }
}