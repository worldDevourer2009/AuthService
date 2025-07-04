using AuthService.Application.Commands.CommandHandlers.Auth;

namespace AuthService.Application.Validators.Commands.Auth;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .Length(8, 30);
    }
}