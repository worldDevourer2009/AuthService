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
using Microsoft.Extensions.Configuration;
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
            var config = sp.GetRequiredService<IConfiguration>();
            var connString = config.GetConnectionString("Redis")
                             ?? throw new InvalidOperationException("Redis connection string is not configured");
            return new RedisSettings { ConnectionString = connString };
        });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var settings = sp.GetRequiredService<RedisSettings>();
            var config = ConfigurationOptions.Parse(settings.ConnectionString!);
            config.AbortOnConnectFail = false;
            config.ConnectRetry = 3;
            config.AsyncTimeout = 5000;
            return ConnectionMultiplexer.Connect(config);
        });

        services.AddSingleton(sp =>
            sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase()
        );

        services.AddScoped<IRedisService, RedisService>();
    }

    private static void BindUserServices(IServiceCollection services)
    {
        services.AddScoped<IUserSignUpService, UserSignUpService>();
        services.AddScoped<IUserLoginService, UserLoginService>();
        services.AddScoped<IUserLogoutService, UserLogoutService>();
    }
}