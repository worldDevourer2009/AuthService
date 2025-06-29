using AuthService.Domain.Entries;
using AuthService.Domain.Responses;

namespace AuthService.Domain.Services.Users;

public interface IUserLoginService
{
    Task<LoginResponse> LoginAsync(LoginEntry entry, CancellationToken cancellationToken = default);
}