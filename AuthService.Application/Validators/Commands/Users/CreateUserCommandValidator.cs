using AuthService.Application.Commands.CommandHandlers.Users;

namespace AuthService.Application.Validators.Commands.Users;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(2, 160);
        
        RuleFor(x => x.LastName)
            .NotEmpty()
            .Length(2, 160);
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .Length(8, 30);
    }
}