using AuthService.Application.Options;
using AuthService.Application.Services;
using AuthService.Application.Services.Repositories;
using AuthService.Domain.Services.Passwords;
using AuthService.Domain.Services.Tokens;
using AuthService.Domain.Services.Users;
using AuthService.Infrastructure.Interfaces;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Persistence.Redis;
using AuthService.Infrastructure.Persistence.Repositories;
using AuthService.Infrastructure.Services.Passwords;
using AuthService.Infrastructure.Services.Tokens;
using AuthService.Infrastructure.Services.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace AuthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IApplicationDbContext, AppDbContext>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDataSeeder, DataSeeder>();
        
        services.AddScoped<IPasswordService, PasswordService>();
        
        services.AddSingleton<IKeyGenerator>(serviceProvider =>
        {
            var env = serviceProvider.GetRequiredService<IHostEnvironment>();
            var logger = serviceProvider.GetRequiredService<ILogger<RSAKeyGen>>();
            var settings = serviceProvider.GetRequiredService<IOptions<RsaKeySettings>>();
            return new RSAKeyGen(env, settings, logger);
        });
        
        services.AddScoped<ITokenService, TokenService>();
        
        BindUserServices(services);
        BindRedis(services);
        return services;
    }

    private static void BindRedis(IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<RedisSettings>>();
            return settings.Value;
        });
        
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var settings = sp.GetRequiredService<RedisSettings>();
            return ConnectionMultiplexer.Connect(settings.ConnectionString!);
        });

        services.AddSingleton(sp =>
        {
            var connectionMultiplexer = sp.GetRequiredService<IConnectionMultiplexer>();

            if (!connectionMultiplexer.IsConnected)
            {
                throw new Exception("Redis connection is not connected");
            }
            
            return connectionMultiplexer.GetDatabase();
        });
        
        services.AddScoped<IRedisService, RedisService>();
    }

    private static void BindUserServices(IServiceCollection services)
    {
        services.AddScoped<IUserSignUpService, UserSignUpService>();
        services.AddScoped<IUserLoginService, UserLoginService>();
        services.AddScoped<IUserLogoutService, UserLogoutService>();
    }
}