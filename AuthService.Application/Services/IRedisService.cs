namespace AuthService.Application.Services;

public interface IRedisService
{
    Task SetAsync(string key, string value, CancellationToken cancellation = default);
    Task SetAsync(string key, string value, TimeSpan expiresAt, CancellationToken cancellation = default);
    Task<string?> GetAsync(string key, CancellationToken cancellation = default);
    Task RemoveAsync(string key, CancellationToken cancellation = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellation = default);
    Task SetExpireAsync(string key, TimeSpan timeSpan, CancellationToken cancellation = default);
}