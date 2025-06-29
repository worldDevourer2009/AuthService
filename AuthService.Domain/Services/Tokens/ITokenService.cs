using AuthService.Domain.Entities.Users;

namespace AuthService.Domain.Services.Tokens;

public interface ITokenService
{
    Task<(string accessToken, string refreshToken)> GenerateTokenPairForUser(string email, CancellationToken cancellationToken = default);
    Task<string> GenerateAccessTokenForUser(string email, CancellationToken cancellationToken = default);
    Task<string> GenerateRefreshTokenForUser(string email, CancellationToken cancellationToken = default);
    Task<bool> RevokeAccessTokenForUser(string email, CancellationToken cancellationToken = default);
    Task<bool> RevokeRefreshTokenForUser(string email, CancellationToken cancellationToken = default);
    Task<bool> RevokeAllTokensForUser(string email, CancellationToken cancellationToken = default);
    Task<bool> IsAccessTokenRevokedForUser(string accessToken, CancellationToken cancellationToken = default);
    Task<bool> IsRefreshTokenRevokedForUser(string email, CancellationToken cancellationToken = default);
    Task<User> GetUserByRefreshToken(string? refreshToken, CancellationToken cancellationToken = default);
}