namespace AuthService.Domain.DomainEvents;

public abstract class DomainEvent(string eventType) : IDomainEvent
{
    public Guid? Id { get; } = Guid.NewGuid();
    public DateTime? CreatedAt { get; } = DateTime.UtcNow;
    public string EventType { get; } = eventType;
}