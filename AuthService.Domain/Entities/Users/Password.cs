using AuthService.Domain.Services.Passwords;

namespace AuthService.Domain.Entities.Users;

public class Password : ValueObject
{
    public string? PasswordHash { get; private set; }
    
    private Password()
    {
    }

    public static Password Create(string? passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentNullException(nameof(passwordHash));
        }
        
        return new Password
        {
            PasswordHash = PasswordHasher.Instance.HashPassword(passwordHash),
        };
    }
    
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PasswordHash;
    }
}