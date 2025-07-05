namespace AuthService.Domain.DomainEvents.Auth;

public class UserSignedUpDomainEvent : DomainEvent
{
    public string Username { get; private set; }
    public string Email { get; private set; }
    public DateTime? SignedUpAt { get; private set; }
    
    public UserSignedUpDomainEvent(string eventType, string username, string email) 
        : base(eventType)
    {
        Username = username;
        Email = email;
        SignedUpAt = DateTime.UtcNow;
    }
}