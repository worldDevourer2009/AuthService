using AuthService.Domain.Responses;

namespace AuthService.Domain.Services.Users;

public interface IUserLogoutService
{
    Task<LogoutResponse> LogoutAsync(string? email, CancellationToken cancellationToken = default);
}