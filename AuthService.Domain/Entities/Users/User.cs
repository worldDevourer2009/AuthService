namespace AuthService.Domain.Entities.Users;

public class User : ValueObject
{
    public UserIdentity UserIdentity { get; private set; }
    public Email Email { get; private set; }
    public Password Password { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public DateTime? LastLogin { get; private set; }
    public bool? IsActive { get; private set; }

    private User()
    {
    }

    public static User Create(string firstName, string lastName, string email, string password)
    {
        return new User
        {
            UserIdentity = UserIdentity.Create(firstName, lastName),
            Email = Email.Create(email),
            Password = Password.Create(password),
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow,
            IsActive = false,
        };
    }

    public void UpdateLastLogin()
    {
        LastLogin = DateTime.UtcNow;
    }

    public void SetIpAddress(string ipAddress)
    {
        IpAddress = ipAddress;
    }

    public void SetNewPassword(string password)
    {
        Password = Password.Create(password);
    }

    public void SetInactive()
    {
        IsActive = false;
    }
    
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return UserIdentity;
        yield return Email;
        yield return IpAddress;
    }
}