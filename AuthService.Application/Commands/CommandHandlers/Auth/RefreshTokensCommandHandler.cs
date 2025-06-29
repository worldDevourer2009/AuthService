using AuthService.Domain.Services.Tokens;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.CommandHandlers.Auth;

public record RefreshTokensRequest(string? RefreshToken) : ICommand<RefreshTokensResponse>;
public record RefreshTokensResponse(bool Success, string? AccessToken = null, string? RefreshToken = null, string? Message = null);

public class RefreshTokensCommandHandler : ICommandHandler<RefreshTokensRequest, RefreshTokensResponse>
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<RefreshTokensCommandHandler> _logger;

    public RefreshTokensCommandHandler(ITokenService tokenService, ILogger<RefreshTokensCommandHandler> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<RefreshTokensResponse> Handle(RefreshTokensRequest request, CancellationToken cancellationToken)
    {
        
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return new RefreshTokensResponse(false, Message: "Refresh token is required");
        }

        try
        {
            var user = await _tokenService.GetUserByRefreshToken(request.RefreshToken, cancellationToken);

            if (user! == null)
            {
                return new RefreshTokensResponse(false, Message: "User was not found");
            }
            
            try
            {
                if (await _tokenService.IsRefreshTokenRevokedForUser(user.Email.EmailAddress!, cancellationToken))
                {
                    return new RefreshTokensResponse(false, Message: "Refresh token is revoked");
                }

                var cortage = await _tokenService.GenerateTokenPairForUser(user.Email.EmailAddress!, cancellationToken);
                return new RefreshTokensResponse(true, cortage.accessToken, cortage.refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Something went wring while generating accessToken");
                return new RefreshTokensResponse(false, Message: "Something went wrong");
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Can't find user from refresh token");
            return new RefreshTokensResponse(false, Message: $"Caught exception {ex.Message}");
        }
    }
}