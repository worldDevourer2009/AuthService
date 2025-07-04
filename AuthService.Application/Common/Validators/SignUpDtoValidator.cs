using TaskHandler.Shared.DTO.Auth;

namespace AuthService.Application.Common.Validators;

public class SignUpDtoValidator : AbstractValidator<SignUpDto>
{
    public SignUpDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Invalid email")
            .EmailAddress();
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password should have between 8 and 30 characters")
            .Length(8, 30);
        
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Name can't be that short")
            .Length(2, 160);
        
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name can't be that short")
            .Length(2, 160);
    }
}