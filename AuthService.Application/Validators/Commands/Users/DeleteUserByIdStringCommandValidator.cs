using AuthService.Application.Commands.CommandHandlers.Users;

namespace AuthService.Application.Validators.Commands.Users;

public class DeleteUserByIdStringCommandValidator : AbstractValidator<DeleteUserByIdStringCommand>
{
    public DeleteUserByIdStringCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotNull();
    }
}