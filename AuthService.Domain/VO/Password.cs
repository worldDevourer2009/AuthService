using System.Text.RegularExpressions;
using AuthService.Domain.Entities;
using AuthService.Domain.Exceptions.VO;
using AuthService.Domain.Services.Passwords;

namespace AuthService.Domain.VO;

public partial class Password : ValueObject
{
    public string? PasswordHash { get; private set; }
    
    private Password()
    {
    }

    public static Password Create(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidPasswordException("Invalid password data");
        }
        
        if (password.Length < 8)
        {
            throw new InvalidPasswordException("Password should be at least 8 characters");
        }

        if (password.Length > 80)
        {
            throw new InvalidPasswordException("Password should be less then 80 characters");
        }
        
        if (!UppercaseRegex().IsMatch(password))
        {
            throw new InvalidPasswordException("Password should contain at least one uppercase letter");
        }

        if (!LowercaseRegex().IsMatch(password))
        {
            throw new InvalidPasswordException("Password should contain at least one lowercase letter");
        }

        if (!DigitRegex().IsMatch(password))
        {
            throw new InvalidPasswordException("Password should contain at least one digit");
        }

        if (!SpecialCharsRegex().IsMatch(password))
        {
            throw new InvalidPasswordException("Password should contain at least one special character");
        }
        
        return new Password
        {
            PasswordHash = PasswordHasher.Instance.HashPassword(password!),
        };
    }
    
    public static Password FromHash(string passwordHash)
    {
        return new Password
        {
            PasswordHash = passwordHash,
        };
    }
    
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PasswordHash;
    }

    [GeneratedRegex(@"[\W_]")]
    private static partial Regex SpecialCharsRegex();
    [GeneratedRegex(@"\d")]
    private static partial Regex DigitRegex();
    [GeneratedRegex(@"[a-z]")]
    private static partial Regex LowercaseRegex();
    [GeneratedRegex(@"[A-Z]")]
    private static partial Regex UppercaseRegex();
}