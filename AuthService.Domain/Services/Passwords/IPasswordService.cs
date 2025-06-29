namespace AuthService.Domain.Services.Passwords;

public interface IPasswordService
{
    Task<bool> VerifyPasswordForUser(string password, string passwordHash);
    Task<bool> ResetPasswordForUser(string email, string oldPassword, string newPassword);
}