namespace AuthService.Application.Services;

public interface IUserService
{
    Task<bool> CreateUser(User? user, CancellationToken cancellationToken = default);
    
    Task<bool> UpdateUser(User? user, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteUserByEmail(string email, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserBydId(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserBydId(string id, CancellationToken cancellationToken = default);
    
    Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserById(Guid id, CancellationToken cancellationToken = default);
}