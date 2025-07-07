using TaskHandler.Shared.Auth.DTO.Auth.AuthResults;

namespace AuthService.Application.Common.Validators;

public class GenerateInternalTokenDtoValidator : AbstractValidator<GenerateInternalTokenDto>
{
    public GenerateInternalTokenDtoValidator()
    {
        RuleFor(x => x.ServiceClientId)
            .NotEmpty()
            .NotNull();
    }
}