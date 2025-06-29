namespace AuthService.Domain.Entities.Users;

public class Email : ValueObject
{
    public string? EmailAddress { get; private set; }
    
    private Email()
    {
    }

    public static Email Create(string? emailAddress)
    {
        if (emailAddress == null || !emailAddress.Contains('@'))
        {
            throw new Exception("Invalid email");
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
}