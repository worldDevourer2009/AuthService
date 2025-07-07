using AuthService.Application.Interfaces;
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
    private readonly IDomainDispatcher _domainDispatcher;

    public UserLoginService(IUserRepository userRepository, IPasswordService passwordService,
        ITokenService tokenService, ILogger<UserLoginService> logger, IDomainDispatcher domainDispatcher)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _logger = logger;
        _domainDispatcher = domainDispatcher;
    }

    public async Task<LoginResponse> LoginAsync(LoginEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entry.Email) || string.IsNullOrWhiteSpace(entry.Password))
            {
                return new LoginResponse(false, "Invalid data");
            }
            
            var user = await _userRepository.GetUserByEmail(entry.Email!, cancellationToken);

            if (user == null)
            {
                return new LoginResponse(false, $"User with email {entry.Email} does not exist");
            }

            if (!await _passwordService.VerifyPasswordForUser(entry.Password, user.Password.PasswordHash!))
            {
               return new LoginResponse(false, "Invalid password");
            }

            var (accessToken, refreshToken) =
                await _tokenService.GenerateTokenPairForUser(user.Email.EmailAddress!, cancellationToken);

            user.UpdateLastLogin();
            
            await _userRepository.UpdateUser(user, cancellationToken);
            await _domainDispatcher.DispatchAsync(user.DomainEvents, cancellationToken);
            user.ClearDomainEvents();
            return new LoginResponse(true, accessToken, refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while logging in user");
            return new LoginResponse(false);
        }
    }
}