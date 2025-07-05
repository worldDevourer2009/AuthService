using AuthService.Application.Commands.CommandHandlers.Auth;

namespace AuthService.Application.Validators.Commands.Auth;

public class SignUpCommandValidator : AbstractValidator<SignUpCommand>
{
    public SignUpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .Length(8, 30);
        
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(2, 160);
        
        RuleFor(x => x.LastName)
            .NotEmpty()
            .Length(2, 160);
    }
}