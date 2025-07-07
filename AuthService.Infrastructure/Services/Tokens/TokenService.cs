using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AuthService.Application.Options;
using AuthService.Application.Services;
using AuthService.Application.Services.Repositories;
using AuthService.Domain.Services.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Services.Tokens;

public class TokenService : ITokenService
{
    private static readonly TimeSpan AccessTokenLifeTime = TimeSpan.FromMinutes(60);
    private static readonly TimeSpan RefreshTokenLifeTime = TimeSpan.FromDays(7);

    private const string DenylistKeyKey = "denylist:jti";
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";

    private readonly IUserRepository _context;
    private readonly IKeyGenerator _keyGenerator;
    private readonly IRedisService _redisService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<TokenService> _logger;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

    public TokenService(IUserRepository context, IKeyGenerator keyGenerator, IRedisService redisService,
        ILogger<TokenService> logger, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _keyGenerator = keyGenerator;
        _redisService = redisService;
        _logger = logger;
        _jwtSettings = jwtSettings.Value;
        _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    }

    public async Task<(string accessToken, string refreshToken)> GenerateTokenPairForUser(string email,
        CancellationToken cancellationToken = default)
    {
        var accessToken = await GenerateAccessTokenForUser(email, cancellationToken);
        var refreshToken = await GenerateRefreshTokenForUser(email, cancellationToken);
        return (accessToken, refreshToken);
    }

    public async Task<string> GenerateAccessTokenForUser(string email, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByEmail(email, cancellationToken);
        var creds = new SigningCredentials(new RsaSecurityKey(_keyGenerator.Rsa), SecurityAlgorithms.RsaSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var jwt = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            claims: claims,
            audience: _jwtSettings.Audience,
            expires: DateTime.UtcNow.Add(AccessTokenLifeTime),
            signingCredentials: creds);

        var accessToken = _jwtSecurityTokenHandler.WriteToken(jwt);
        _logger.LogInformation("Generated access token for user successfully");
        return accessToken;
    }

    public async Task<string> GenerateRefreshTokenForUser(string email, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByEmail(email, cancellationToken);

        var randomNumber = new byte[64];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);
        var redisKey = GetTokenKey(RefreshTokenKey, user.Id.ToString()!);

        await _redisService.SetAsync(redisKey, refreshToken, RefreshTokenLifeTime, cancellationToken);
        await _redisService.SetAsync(GetUserIdKey(refreshToken), user.Id.ToString()!, RefreshTokenLifeTime,
            cancellationToken);
        _logger.LogInformation("Generated refresh token for user successfully");
        return refreshToken;
    }

    public async Task<bool> RevokeAccessTokenForUser(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = _jwtSecurityTokenHandler.ReadJwtToken(accessToken);
            var jti = token.Id;

            if (string.IsNullOrWhiteSpace(jti))
            {
                _logger.LogWarning("Jti is empty");
                return false;
            }

            var expires = token.ValidTo;

            if (expires < DateTime.UtcNow)
            {
                return true;
            }

            var validLifeTime = expires - DateTime.UtcNow;

            var redisKey = $"{DenylistKeyKey}:{jti}";
            await _redisService.SetAsync(redisKey, "revoked", validLifeTime, cancellationToken);
            _logger.LogInformation("Access token revoked successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove access key");
            return false;
        }
    }

    public async Task<bool> RevokeRefreshTokenForUser(string email, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByEmail(email, cancellationToken);

        try
        {
            var redisKey = GetTokenKey(RefreshTokenKey, user.Id.ToString()!);
            var oldToken = await _redisService.GetAsync(redisKey, cancellationToken);

            var lookupKey = GetUserIdKey(oldToken!);
            await _redisService.RemoveAsync(lookupKey, cancellationToken);
            await _redisService.RemoveAsync(redisKey, cancellationToken);
            _logger.LogInformation("Refresh token revoked successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove refresh key");
            return false;
        }
    }

    public async Task<bool> RevokeAllTokensForUser(string email, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByEmail(email, cancellationToken);

        try
        {
            var refreshRedisKey = GetTokenKey(RefreshTokenKey, user.Id.ToString()!);
            var accessRedisKey = GetTokenKey(AccessTokenKey, user.Id.ToString()!);
            await _redisService.RemoveAsync(refreshRedisKey, cancellationToken);
            await _redisService.RemoveAsync(accessRedisKey, cancellationToken);
            _logger.LogInformation("All tokens revoked successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while removing tokens for user");
            return false;
        }
    }

    public async Task<bool> IsAccessTokenRevokedForUser(string accessToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var jti = _jwtSecurityTokenHandler.ReadJwtToken(accessToken);

            if (string.IsNullOrWhiteSpace(jti.Id))
            {
                _logger.LogWarning("Invalid jwt token");
                return false;
            }

            var redisKey = $"{DenylistKeyKey}:{jti.Id}";
            var result = await _redisService.ExistsAsync(redisKey, cancellationToken);
            _logger.LogInformation("Checking revoked access token {Result}", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Something went wrong while checking revoked access token");
            return false;
        }
    }

    public async Task<bool> IsRefreshTokenRevokedForUser(string email, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByEmail(email, cancellationToken);

        try
        {
            var refreshRedisKey = GetTokenKey(RefreshTokenKey, user.Id.ToString()!);
            var result = !await _redisService.ExistsAsync(refreshRedisKey, cancellationToken);
            _logger.LogInformation("Checking revoked refresh token {Result}", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while checking existence of refreshToken");
            return false;
        }
    }

    public async Task<User?> GetUserByRefreshToken(string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogError("Refresh token is null");
            throw new ArgumentException("RefreshToken can't be null");
        }

        var id = await _redisService.GetAsync(GetUserIdKey(refreshToken), cancellationToken);

        if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var parsedId))
        {
            _logger.LogError("User id is null");
            throw new ArgumentException("User id is null");
        }

        var user = await _context.GetUserById(parsedId, cancellationToken);
        _logger.LogInformation("User {UserId} found", id);
        return user;
    }

    public Task<string?> GenerateToken(string issuer, string audience, IEnumerable<Claim> claims, DateTime expiresIn,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;
            var creds = new SigningCredentials(new RsaSecurityKey(_keyGenerator.Rsa), SecurityAlgorithms.RsaSha256);

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: expiresIn,
                signingCredentials: creds);

            var token = _jwtSecurityTokenHandler.WriteToken(jwt);
            _logger.LogInformation("Generated access token for service successfully");
            return Task.FromResult<string?>(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating token for service");
            return Task.FromResult<string?>(null);
        }
    }

    private async Task<User> GetUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        var user = await _context.GetUserByEmail(email, cancellationToken);

        if (user == null)
        {
            _logger.LogError("User not found in {Method}", nameof(GetUserByEmail));
            throw new Exception("User not found");
        }

        return user;
    }

    private string GetTokenKey(string tokenKey, string userId)
    {
        return $"{tokenKey}:{userId}";
    }

    private string GetUserIdKey(string refreshToken)
    {
        return $"refresh_token_lookup:{refreshToken}";
    }
}