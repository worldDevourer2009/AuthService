namespace AuthService.Domain.Services.Passwords;

public interface IPasswordService
{
    Task<bool> VerifyPasswordForUser(string email, string password);
    Task<bool> ResetPasswordForUser(string email, string oldPassword, string newPassword);
}