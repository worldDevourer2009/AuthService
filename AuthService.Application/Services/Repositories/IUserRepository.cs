namespace AuthService.Application.Services.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserById(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default);
    Task AddNewUser(User user, CancellationToken cancellationToken = default);
    Task UpdateUser(User user, CancellationToken cancellationToken = default);
    Task DeleteUser(User user, CancellationToken cancellationToken = default);
    Task DeleteUserByEmail(string? email, CancellationToken cancellationToken = default);
    Task DeleteUserById(Guid id, CancellationToken cancellationToken = default);
}