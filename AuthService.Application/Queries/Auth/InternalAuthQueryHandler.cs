using System.Security.Claims;
using AuthService.Application.Options;
using AuthService.Domain.Services.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Application.Queries.Auth;

public record InternalAuthQuery(IEnumerable<Claim> Claims) : IQuery<InternalAuthResult>;
public record InternalAuthResult(bool Success, string? Token = null, string? Message = null);

public class InternalAuthQueryHandler : IQueryHandler<InternalAuthQuery, InternalAuthResult>
{
    private readonly ITokenService _tokenService;
    private readonly InternalAuth _internalAuth;
    private readonly ILogger<InternalAuthQueryHandler> _logger;

    public InternalAuthQueryHandler(ITokenService tokenService, IOptions<InternalAuth> internalAuth,
        ILogger<InternalAuthQueryHandler> logger)
    {
        _tokenService = tokenService;
        _internalAuth = internalAuth.Value;
        _logger = logger;
    }

    public async Task<InternalAuthResult> Handle(InternalAuthQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _tokenService.GenerateToken(
                _internalAuth.Issuer,
                _internalAuth.Audience,
                request.Claims,
                DateTime.UtcNow.Add(TimeSpan.FromMinutes(_internalAuth.AccessTokenExpirationMinutes)),
                cancellationToken);

            return result is null
                ? new InternalAuthResult(false, null,"Can't generate token")
                : new InternalAuthResult(true, result, "Token generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something happened while generating internal token");
            return new InternalAuthResult(false, null,"Can't generate token");
        }
    }
}