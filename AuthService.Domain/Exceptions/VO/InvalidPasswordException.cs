namespace AuthService.Domain.Exceptions.VO;

public class InvalidPasswordException : DomainException
{
    public string Reason { get; }

    public InvalidPasswordException(string reason) :
        base($"Can't generate password because {reason}")
    {
        Reason = reason;
    }
}