namespace AuthService.Domain.DomainEvents.Auth;

public class UserLoggedInDomainEvent : DomainEvent
{
    public string Username { get; private set; }
    public string Email { get; private set; }
    public DateTime? LastLogin { get; private set; }
    
    public UserLoggedInDomainEvent(string eventType, string username, string email) 
        : base(eventType)
    {
        Username = username;
        Email = email;
        LastLogin = DateTime.UtcNow; 
    }
}