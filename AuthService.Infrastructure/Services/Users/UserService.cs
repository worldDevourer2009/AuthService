using AuthService.Application.Services;
using AuthService.Application.Services.Repositories;
using AuthService.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserRepository> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserRepository> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<bool> CreateUser(User? user, CancellationToken cancellationToken = default)
    {
        try
        {
            if (user is null)
            {
                _logger.LogError($"User is null {nameof(CreateUser)}");
                return false;
            }
            
            await _userRepository.AddNewUser(user, cancellationToken);
            _logger.LogInformation("User {UserEmail} created", user.Email);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Something went wrong while creating user");
            return false;
        }
    }

    public async Task<bool> UpdateUser(User? user, CancellationToken cancellationToken = default)
    {
        try
        {
            if (user is null)
            {
                _logger.LogError($"User is null {nameof(UpdateUser)}");
                return false;
            }
            
            await _userRepository.UpdateUser(user, cancellationToken);
            _logger.LogInformation("User {UserEmail} updated", user.Email);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Something went wrong while creating user");
            return false;
        }
    }

    public async Task<bool> DeleteUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogError($"Email is null {nameof(DeleteUserByEmail)}");
                return false;
            }

            await _userRepository.DeleteUserByEmail(email, cancellationToken);
            _logger.LogInformation("User {UserEmail} deleted", email);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Something went wrong while creating user {email}");
            return false;
        }
    }

    public async Task<bool> DeleteUserBydId(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id == Guid.Empty)
            {
                _logger.LogError($"Id is null {nameof(DeleteUserBydId)}");
                return false;
            }
            
            await _userRepository.DeleteUserById(id, cancellationToken);
            _logger.LogInformation("User {UserId} deleted", id);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Something went wrong while creating user {id}");
            return false;
        }
    }

    public async Task<bool> DeleteUserBydId(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogError($"Id is null {nameof(DeleteUserBydId)}");
                return false;
            }

            if (!Guid.TryParse(id, out var parsedId))
            {
                _logger.LogError($"Id is not valid {nameof(DeleteUserBydId)}");
                return false;
            }
            
            await _userRepository.DeleteUserById(parsedId, cancellationToken);
            _logger.LogInformation("User {UserId} deleted", id);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Something went wrong while deleting user by id {id}");
            return false;
        }
    }

    public async Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogError($"Email is null {nameof(GetUserByEmail)}");
                return null;
            }
            
            var user = await _userRepository.GetUserByEmail(email, cancellationToken);

            if (user is null)
            {
                _logger.LogError($"User with email {email} does not exist");
                return null;
            }

            _logger.LogInformation("User {UserEmail} found", email);
            return user;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Something went wrong while getting user by email {email}");
            return null;
        }
    }

    public async Task<User?> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id == Guid.Empty)
            {
                _logger.LogError($"Id is null {nameof(GetUserById)}");
                return null;
            }
            
            var user = await _userRepository.GetUserById(id, cancellationToken);

            if (user is null)
            {
                _logger.LogError($"User with id {id} does not exist");
                return null;
            }
            
            _logger.LogInformation("User {UserId} found", id);
            return user;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Something went wrong while getting user by id {id}");
            return null;
        }
    }
}