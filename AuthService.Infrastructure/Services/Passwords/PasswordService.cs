using AuthService.Application.Services.Repositories;
using AuthService.Domain.Services.Passwords;

namespace AuthService.Infrastructure.Services.Passwords;

public class PasswordService : IPasswordService
{
    private readonly IUserRepository _context;

    public PasswordService(IUserRepository context)
    {
        _context = context;
    }

    public Task<bool> VerifyPasswordForUser(string password, string passwordHash)
    {
        return Task.FromResult(PasswordHasher.Instance.VerifyPassword(password, passwordHash));
    }

    public async Task<bool> ResetPasswordForUser(string email, string oldPassword, string newPassword)
    {
        var user = await GetUserByEmail(email);

        if (!PasswordHasher.Instance.VerifyPassword(oldPassword, user.Password.PasswordHash!))
        {
            return false;
        }
        
        var newPasswordHash = PasswordHasher.Instance.HashPassword(newPassword);
        user.SetNewPassword(newPasswordHash);

        try
        {
            await _context.UpdateUser(user);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<User> GetUserByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentNullException(nameof(email));
        }
        
        var user = await _context.GetUserByEmail(email);
        
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        return user;
    }
}