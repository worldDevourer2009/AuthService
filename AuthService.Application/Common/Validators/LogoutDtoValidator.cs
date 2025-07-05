using TaskHandler.Shared.Auth.DTO.Auth;

namespace AuthService.Application.Common.Validators;

public class LogoutDtoValidator : AbstractValidator<LogoutDto>
{
    public LogoutDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Invalid Email")
            .EmailAddress()
            .WithMessage("Invalid Email");

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Every user should have a valid token");
    }
}