using AuthService.Application.Services.Repositories;
using AuthService.Domain.Entries;
using AuthService.Domain.Responses;
using AuthService.Domain.Services.Passwords;
using AuthService.Domain.Services.Tokens;
using AuthService.Domain.Services.Users;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services.Users;

public class UserLoginService : IUserLoginService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<UserLoginService> _logger;
    private readonly ITokenService _tokenService;

    public UserLoginService(IUserRepository userRepository, IPasswordService passwordService, ITokenService tokenService, ILogger<UserLoginService> logger)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginEntry entry, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetUserByEmail(entry.Email!, cancellationToken);

        if (user! == null)
        {
            return new LoginResponse(false);
        }

        if (!await _passwordService.VerifyPasswordForUser(user.Email.EmailAddress!, entry.Password!))
        {
            return new LoginResponse(false);
        }

        try
        {
            var (accessToken, refreshToken) =
                await _tokenService.GenerateTokenPairForUser(user.Email.EmailAddress!, cancellationToken);
            
            user.UpdateLastLogin();
        
            return new LoginResponse(true, accessToken, refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while logging in user");
            return new LoginResponse(false);
        }
    }
}