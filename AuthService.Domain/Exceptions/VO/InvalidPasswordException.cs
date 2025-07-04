namespace AuthService.Domain.Exceptions.VO;

public class InvalidPasswordException : Exception
{
    public string Reason { get; }

    public InvalidPasswordException(string reason) :
        base($"Can't generate password because {reason}")
    {
        Reason = reason;
    }
}