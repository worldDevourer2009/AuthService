using AuthService.Application.DTOtoTransfer;

namespace AuthService.Application.Common.Validators;

public class GenerateInternalTokenDtoValidator : AbstractValidator<Classes.GenerateInternalTokenDto>
{
    public GenerateInternalTokenDtoValidator()
    {
        RuleFor(x => x.ServiceClientId)
            .NotEmpty()
            .NotNull();
    }
}