namespace AuthService.Domain.DomainEvents.Auth;

public class UserLoggedOutDomainEvent : DomainEvent
{
    public string Username { get; private set; }
    public string Email { get; private set; }
    public DateTime? LastLogout { get; private set; }
    
    public UserLoggedOutDomainEvent(string eventType, string username, string email) 
        : base(eventType)
    {
        Username = username;
        Email = email;
        LastLogout = DateTime.UtcNow;
    }
}