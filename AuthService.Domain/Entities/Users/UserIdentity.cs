using AuthService.Domain.Exceptions.Entities;

namespace AuthService.Domain.Entities.Users;

public class UserIdentity : ValueObject
{
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    
    private UserIdentity()
    {
    }

    public static UserIdentity Create(string firstName, string lastName)
    {
        if (firstName.Length < 2)
        {
            throw new InvalidUserIdentity("First name should be longer");
        }
        
        if (lastName.Length < 2)
        {
            throw new InvalidUserIdentity("Last name should be longer");
        }
        
        return new UserIdentity
        {
            FirstName = firstName,
            LastName = lastName,
        };
    }

    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }

    public void SetFirstName(string firstName)
    {
        FirstName = firstName;
    }

    public void SetLastName(string lastName)
    {
        LastName = lastName;
    }
}