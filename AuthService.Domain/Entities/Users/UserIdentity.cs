namespace AuthService.Domain.Entities.Users;

public class UserIdentity : ValueObject
{
    public Guid? Id { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    
    private UserIdentity()
    {
    }

    public static UserIdentity Create(string firstName, string lastName)
    {
        return new UserIdentity
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
        };
    }

    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }
}