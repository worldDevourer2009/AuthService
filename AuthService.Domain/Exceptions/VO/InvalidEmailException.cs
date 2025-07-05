namespace AuthService.Domain.Exceptions.VO;

public class InvalidEmailException : DomainException
{
    public string Reason { get; }

    public InvalidEmailException(string reason)
        : base($"Can't create email because {reason}")
    {
        Reason = reason;
    }
}