using AuthService.Application.Services;
using StackExchange.Redis;

namespace AuthService.Infrastructure.Persistence.Redis;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;

    public RedisService(IConnectionMultiplexer connectionMultiplexer, IDatabase database)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _database = database;
    }

    public async Task SetAsync(string key, string value, CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();
        
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Key or value cannot be null or whitespace.");
        }

        await _database.StringSetAsync(key, value);
    }

    public async Task SetAsync(string key, string value, TimeSpan expiresAt, CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();
        
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Key or value cannot be null or whitespace.");
        }

        await _database.StringSetAsync(key, value);
        await _database.KeyExpireAsync(key, expiresAt);
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();
        
        if (string.IsNullOrWhiteSpace(key) || await ExistsAsync(key, cancellation) == false)
        {
            throw new ArgumentException("Key cannot be null or whitespace.");
        }
        
        return await _database.StringGetAsync(key);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();
        
        if (string.IsNullOrWhiteSpace(key) || !await ExistsAsync(key, cancellation))
        {
            throw new ArgumentException("Key cannot be null or whitespace.");
        }

        await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();
        return await _database.KeyExistsAsync(key);
    }

    public async Task SetExpireAsync(string key, TimeSpan timeSpan, CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();
        
        if (string.IsNullOrWhiteSpace(key) || timeSpan.TotalSeconds <= 0)
        {
            throw new ArgumentException("Key or timespan cannot be null or whitespace.");
        }
        
        await _database.KeyExpireAsync(key, timeSpan);
    }
}