using TaskHandler.Shared.DTO.Auth;

namespace AuthService.Application.Common.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Invalid email")
            .EmailAddress()
            .WithMessage("Invalid email");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Invalid password")
            .Length(8, 30)
            .WithMessage("Invalid password length");
    }
}