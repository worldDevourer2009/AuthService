using AuthService.Application.Commands.CommandHandlers.Auth;

namespace AuthService.Application.Validators.Commands.Auth;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}