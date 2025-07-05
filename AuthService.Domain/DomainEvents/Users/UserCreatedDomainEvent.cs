namespace AuthService.Domain.DomainEvents.Users;

public class UserCreatedDomainEvent : DomainEvent
{
    public string Username { get; private set; }
    public string Email { get; private set; }

    public UserCreatedDomainEvent(string eventType, string username, string email) : base(eventType)
    {
        Username = username;
        Email = email;
    }
}