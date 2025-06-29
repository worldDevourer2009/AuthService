using AuthService.Application.Services.Repositories;
using AuthService.Domain.Entries;
using AuthService.Domain.Responses;
using AuthService.Domain.Services.Tokens;
using AuthService.Domain.Services.Users;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services.Users;

public class UserSignUpService : IUserSignUpService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UserSignUpService> _logger;

    public UserSignUpService(IUserRepository userRepository, ITokenService tokenService, ILogger<UserSignUpService> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<SignUpResponse> SignUpAsync(SignUpEntry entry, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entry.Email) || string.IsNullOrWhiteSpace(entry.Password) ||
            string.IsNullOrWhiteSpace(entry.FirstName) || string.IsNullOrWhiteSpace(entry.LastName))
        {
            return new SignUpResponse(false);
        }

        try
        {
            if (await _userRepository.GetUserByEmail(entry.Email, cancellationToken) != null!)
            {
                return new SignUpResponse(false);
            }
            
            var user = User.Create(entry.FirstName, entry.LastName, entry.Email, entry.Password);

            await _userRepository.AddNewUser(user, cancellationToken);

            var (accessToken, refreshToken) =
                await _tokenService.GenerateTokenPairForUser(user.Email.EmailAddress!, cancellationToken);

            return new SignUpResponse(true, accessToken, refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex,"Something went wrong while signing up");
            return new SignUpResponse(false);
        }
    }
}