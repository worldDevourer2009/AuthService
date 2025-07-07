using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AuthService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit.Abstractions;

namespace AuthService.Tests.AuthTests;

public class AuthServicesTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly RedisContainer _redisContainer;
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private readonly ITestOutputHelper _output;
    private AppDbContext _context;

    public AuthServicesTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _output = output;

        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("authservice_test")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithCleanUp(true)
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .Build();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => { logging.ClearProviders(); });
        });

        _factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((ctx, conf) =>
            {
                var settings = new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = _postgreSqlContainer.GetConnectionString(),
                    ["ConnectionStrings:Redis"] = $"{_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort(6379)}",
                    ["JwtSettings:Issuer"] = "test-issuer",
                    ["JwtSettings:Audience"] = "test-audience",
                    ["JwtSettings:Key"] = "Nj8s@Z%~4vH8*91@",
                    ["RsaKeySettings:KeyPath"] = "Keys/key.pem",
                    ["RsaKeySettings:GenerateIfMissing"] = "true",
                    ["RsaKeySettings:KeySize"] = "2048",
                    ["InternalAuth:ServiceClientId"] = "test-service-client",
                    ["InternalAuth:ServiceClientSecret"] = "test-service-secret",
                    ["InternalAuth:Issuer"] = "http://auth.internal"
                };
                conf.AddInMemoryCollection(settings);

            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();

                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(_postgreSqlContainer.GetConnectionString(),
                        npgsql => npgsql.MigrationsAssembly("AuthService.Infrastructure")));

                services.AddLogging(log => log.AddConsole().SetMinimumLevel(LogLevel.Debug));
                services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        await _redisContainer.StartAsync();

        var redisReady = false;
        var redisConnString = $"{_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort(6379)}";

        for (var i = 0; i < 10; i++)
        {
            try
            {
                var options = ConfigurationOptions.Parse(redisConnString);
                options.AbortOnConnectFail = false;
                await using var mux = await ConnectionMultiplexer.ConnectAsync(options);

                if (!mux.IsConnected)
                {
                    continue;
                }

                var pong = await mux.GetDatabase().PingAsync();
                redisReady = true;
                break;
            }
            catch
            {
                await Task.Delay(1000);
            }
        }

        if (!redisReady)
        {
            throw new Exception($"Redis container not ready at {redisConnString}");
        }

        var connectionString = _postgreSqlContainer.GetConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly("AuthService.Infrastructure"))
            .Options;

        _context = new AppDbContext(optionsBuilder);
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.StopAsync();
        await _redisContainer.StopAsync();
    }

    [Fact]
    public async Task GetPublicKey_ShouldReturnPemText()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/.well-known/public-key.pem");
        var pem = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("-----BEGIN PUBLIC KEY-----\n", pem);
        _output.WriteLine("Public key length: {0}", pem.Length);
    }

    [Fact]
    private async Task GenerateServiceToken_ValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        var clientId = configuration["InternalAuth:ServiceClientId"];
        var secret = configuration["InternalAuth:ServiceClientSecret"];
        var dto = new { ServiceClientId = clientId, ClientSecret = secret };
        
        // Act
        var response = await client.PostAsJsonAsync("/api/AuthInternal/auth-internal", dto);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var result = root.GetProperty("success");
        var token = root.GetProperty("token").GetString();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result.GetBoolean());
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        _output.WriteLine("Generated token: {0}", token);
    }

    [Theory]
    [InlineData("wrong", "creds")]
    public async Task GenerateServiceToken_InvalidCredentials_ShouldReturnUnauthorized(string clientId, string secret)
    {
        // Arrange
        var client = _factory.CreateClient();
        var dto = new { ServiceClientId = clientId, ClientSecret = secret };
        
        // Act
        var response = await client.PostAsJsonAsync("/api/AuthInternal/auth-internal", dto);
        var body = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        _output.WriteLine("Response: {0}", body);
    }
}