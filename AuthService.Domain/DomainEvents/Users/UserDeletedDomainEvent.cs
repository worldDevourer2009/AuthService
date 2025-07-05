namespace AuthService.Domain.DomainEvents.Users;

public class UserDeletedDomainEvent : DomainEvent
{
    public string Username { get; private set; }
    public string Email { get; private set; }
    
    public UserDeletedDomainEvent(string eventType, string username, string email) 
        : base(eventType)
    {
        Username = username;
        Email = email;
    }
}