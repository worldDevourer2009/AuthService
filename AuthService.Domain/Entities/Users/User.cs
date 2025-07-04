using AuthService.Domain.DomainEvents.Auth;
using AuthService.Domain.VO;

namespace AuthService.Domain.Entities.Users;

public class User : Entity
{
    public UserIdentity UserIdentity { get; private set; }
    public Email Email { get; private set; }
    public Password Password { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public DateTime? LastLogin { get; private set; }
    public bool? IsActive { get; private set; }

    private User() : base()
    {
    }

    public static User Create(string firstName, string lastName, string email, string password)
    {
        var user = new User
        {
            UserIdentity = UserIdentity.Create(firstName, lastName),
            Email = Email.Create(email),
            Password = Password.Create(password),
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow,
            IsActive = false,
        };
        
        user.AddDomainEvent(new UserSignedUpDomainEvent(
            "user-signed-up",
            user.UserIdentity.FirstName!,
            user.Email.EmailAddress!
        ));
        
        return user;
    }

    public void UpdateLastLogin()
    {
        LastLogin = DateTime.UtcNow;
        AddDomainEvent(new UserLoggedInDomainEvent("user-logged-in",
            UserIdentity.FirstName!,
            Email.EmailAddress!));
    }

    public void SetIpAddress(string ipAddress)
    {
        IpAddress = ipAddress;
    }

    public void SetNewPassword(string password)
    {
        Password = Password.Create(password);
    }

    public void SetNewEmail(string email)
    {
        Email = Email.Create(email);
    }

    public void SetInactive()
    {
        IsActive = false;
        
        AddDomainEvent(new UserLoggedOutDomainEvent("user-logged-out",
            UserIdentity.FirstName!,
            Email.EmailAddress!));
    }
}