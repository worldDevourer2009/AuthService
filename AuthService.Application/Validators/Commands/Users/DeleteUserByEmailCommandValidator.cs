using AuthService.Application.Commands.CommandHandlers.Users;

namespace AuthService.Application.Validators.Commands.Users;

public class DeleteUserByEmailCommandValidator : AbstractValidator<DeleteUserByEmailCommand>
{
    public DeleteUserByEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .NotNull()
            .EmailAddress();
    }
}