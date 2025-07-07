using System.Text.RegularExpressions;
using AuthService.Domain.Entities;
using AuthService.Domain.Exceptions.VO;

namespace AuthService.Domain.VO;

public partial class Email : ValueObject
{
    private static readonly Regex EmailRegex = MyRegex();
    
    public string? EmailAddress { get; private set; }
    
    private Email()
    {
    }

    public static Email Create(string? emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress) || !EmailRegex.IsMatch(emailAddress))
        {
            throw new InvalidEmailException("Invalid email format.");
        }
        
        return new Email
        {
            EmailAddress = emailAddress,
        };
    }

    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return EmailAddress;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "fr-US")]
    private static partial Regex MyRegex();
}