using AuthService.Application.Queries.Users;

namespace AuthService.Application.Validators.Queries.Users;

public class GetUserByEmailQueryValidator : AbstractValidator<GetUserByEmailQuery>
{
    public GetUserByEmailQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Invalid email");
    }
}