using System.Security.Claims;
using AuthService.Application.Queries.Auth;

namespace AuthService.Application.Validators.Queries.Auth;

public class InternalAuthQueryValidator : AbstractValidator<InternalAuthQuery>
{
    public InternalAuthQueryValidator()
    {
        RuleFor(x => x.Claims)
            .NotNull()
            .WithMessage("Claims cannot be null")
            .Must(HaveRequiredScopeClaim)
            .WithMessage("Claims must contain 'scope' claim with value 'internal_api'")
            .Must(HaveServiceNameClaim)
            .WithMessage("Claims must contain 'service_name' claim");
    }

    private bool HaveRequiredScopeClaim(IEnumerable<Claim> claims)
    {
        return claims.Any(claim => claim.Type == "scope" && claim.Value == "internal_api");
    }

    private bool HaveServiceNameClaim(IEnumerable<Claim> claims)
    {
        return claims.Any(claim => claim.Type == "service_name" && !string.IsNullOrWhiteSpace(claim.Value));
    }
}