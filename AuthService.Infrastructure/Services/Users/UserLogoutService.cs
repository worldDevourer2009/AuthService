using AuthService.Application.Services.Repositories;
using AuthService.Domain.Responses;
using AuthService.Domain.Services.Tokens;
using AuthService.Domain.Services.Users;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services.Users;

public class UserLogoutService : IUserLogoutService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UserLogoutService> _logger;

    public UserLogoutService(IUserRepository userRepository, ITokenService tokenService, ILogger<UserLogoutService> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LogoutResponse> LogoutAsync(string? email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new LogoutResponse(false, "Email is required");
        }
        
        var user = await _userRepository.GetUserByEmail(email, cancellationToken);

        if (user! == null)
        {
            return new LogoutResponse(false, "User not found");
        }
        
        user.SetInactive();

        try
        {
            await _tokenService.RevokeAllTokensForUser(user.Email.EmailAddress!, cancellationToken);
            await _userRepository.UpdateUser(user, cancellationToken);

            return new LogoutResponse(true, "User logged out");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex,"Something went wrong while logging out user");
            return new LogoutResponse(false, "User was not logged out");
        }
    }
}