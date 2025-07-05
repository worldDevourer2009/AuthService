using AuthService.Application.Commands.CommandHandlers.Users;

namespace AuthService.Application.Validators.Commands.Users;

public class UpdateUserByIdCommandValidator : AbstractValidator<UpdateUserByIdCommand>
{
    public UpdateUserByIdCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.LastName)
            .Length(2, 160)
            .WithMessage("Last name should be longer than 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));
        
        RuleFor(x => x.FirstName)
            .Length(2, 160)
            .WithMessage("Name should be longer than 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Invalid email")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Password)
            .Length(8, 30)
            .WithMessage("Password should be between 8 and 30 characters");
    }
}