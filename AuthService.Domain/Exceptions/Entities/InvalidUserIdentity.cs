namespace AuthService.Domain.Exceptions.Entities;

public class InvalidUserIdentity : Exception
{
    public string Reason { get; }

    public InvalidUserIdentity(string reason)
        : base($"Can't create user because {reason}")
    {
        Reason = reason;
    }
}