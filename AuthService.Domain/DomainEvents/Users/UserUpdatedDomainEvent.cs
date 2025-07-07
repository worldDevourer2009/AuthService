namespace AuthService.Domain.DomainEvents.Users;

public class UserUpdatedDomainEvent(string eventType) : DomainEvent(eventType);