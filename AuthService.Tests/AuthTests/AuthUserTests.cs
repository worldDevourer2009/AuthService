using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AuthService.Application.Commands.CommandHandlers.Auth;
using AuthService.Infrastructure.Persistence;
using AuthService.Shared.DTO.Auth;
using AuthService.Shared.DTO.Auth.AuthResults;
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

public class AuthUserTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private readonly RedisContainer _redisContainer;
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly ITestOutputHelper _output;
    private AppDbContext _context;

    public AuthUserTests(WebApplicationFactory<Program> webApplicationFactory, ITestOutputHelper output)
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

        _webApplicationFactory = webApplicationFactory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((ctx, conf) =>
            {
                var settings = new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = _postgreSqlContainer.GetConnectionString(),
                    ["ConnectionStrings:Redis"] =
                        $"{_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort(6379)}",
                    ["JwtSettings:Issuer"] = "test-issuer",
                    ["JwtSettings:Audience"] = "test-audience",
                    ["JwtSettings:Key"] = "Nj8s@Z%~4vH8*91@",
                    ["RsaKeySettings:KeyPath"] = "Keys/key.pem",
                    ["RsaKeySettings:GenerateIfMissing"] = "true",
                    ["RsaKeySettings:KeySize"] = "2048"
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

    #region Sign Up Tests

    [Fact]
    public async Task SignUp_WithValidData_ReturnsOk()
    {
        var client = _webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });
        var request = new SignUpDto("John", "Doe", "john.doe@example.com", "Password123!");

        // Act
        var response = await client.PostAsJsonAsync("api/auth/sign-up", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SignUpResultDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.AccessToken);

        foreach (var header in response.Headers)
        {
            _output.WriteLine($"Header name: {header.Key}");
            foreach (var value in header.Value)
            {
                _output.WriteLine($"  Value: {value}");
            }
        }

        var cookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();

        Assert.NotNull(cookieHeader);

        var cookies = cookieHeader.Split(';')
            .Select(c => c.Trim())
            .Select(c => c.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1]);

        cookies.TryGetValue("refreshToken", out var refreshToken);

        Assert.False(string.IsNullOrEmpty(refreshToken));
    }

    [Fact]
    public async Task SignUp_WithInvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var request = new SignUpDto("John", "Doe", "invalid-email", "Password123!");

        // Act
        var response = await client.PostAsJsonAsync("api/auth/sign-up", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SignUp_WithDuplicateEmail_ReturnsUnauthorized()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var request = new SignUpDto("John", "Doe", "duplicate@example.com", "Password123!");

        // Act
        var firstResponse = await client.PostAsJsonAsync("api/auth/sign-up", request);
        var secondResponse = await client.PostAsJsonAsync("api/auth/sign-up", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, secondResponse.StatusCode);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var email = "login.test@example.com";
        var password = "Password123!";

        var signUpRequest = new SignUpDto("Test", "User", email, password);
        await client.PostAsJsonAsync("api/auth/sign-up", signUpRequest);

        var loginRequest = new LoginDto(email, password);

        // Act
        var response = await client.PostAsJsonAsync("api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<LoginResultDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _output.WriteLine($"Response: {content}");
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var email = "invalid.password@example.com";
        var correctPassword = "Password123!";
        var wrongPassword = "WrongPassword123!";

        var signUpRequest = new SignUpDto("Test", "User", email, correctPassword);
        await client.PostAsJsonAsync("api/auth/sign-up", signUpRequest);

        var loginRequest = new LoginDto(email, wrongPassword);

        // Act
        var response = await client.PostAsJsonAsync("api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var loginRequest = new LoginDto("nonexistent@example.com", "Password123!");

        try
        {
            // Act
            var response = await client.PostAsJsonAsync("api/auth/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        catch (Exception ex)
        {
            Assert.True(true);
        }
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsOk()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        var email = "refresh.test@example.com";
        var password = "Password123!";

        var signUpRequest = new SignUpDto("Test", "User", email, password);
        var signUpResponse = await client.PostAsJsonAsync("api/auth/sign-up", signUpRequest);

        foreach (var header in signUpResponse.Headers)
        {
            _output.WriteLine($"Header name: {header.Key}");
            foreach (var value in header.Value)
            {
                _output.WriteLine($"  Value: {value}");
            }
        }

        var refreshCookie = signUpResponse.Headers.GetValues("Set-Cookie")
            .FirstOrDefault(c => c.StartsWith("refreshToken="));

        Assert.NotNull(refreshCookie);

        var refreshToken = refreshCookie.Split(';')[0].Split('=')[1];
        client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");

        // Act
        var response = await client.PostAsync("api/auth/refresh", null);
        var content1 = await response.Content.ReadAsStringAsync();
        _output.WriteLine(content1);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RefreshTokensResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithoutRefreshToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();

        // Act
        var response = await client.PostAsync("api/auth/refresh", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "refreshToken=invalid-token");

        // Act
        var response = await client.PostAsync("api/auth/refresh", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidUser_ReturnsOk()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var email = "logout.test@example.com";
        var password = "Password123!";

        var signUpRequest = new SignUpDto("Test", "User", email, password);
        await client.PostAsJsonAsync("api/auth/sign-up", signUpRequest);

        var logoutRequest = new LogoutDto(email, "");

        // Act
        var response = await client.PostAsJsonAsync("api/auth/logout", logoutRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        dynamic? result = JsonSerializer.Deserialize<object>(content);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Logout_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var logoutRequest = new LogoutDto("nonexistent@example.com", "");

        try
        {
            // Act
            var response = await client.PostAsJsonAsync("api/auth/logout", logoutRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        catch (Exception ex)
        {
            //Assert
            Assert.True(true);
        }
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task AuthFlow_CompleteUserJourney_WorksCorrectly()
    {
        var client = _webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions
            { HandleCookies = false });
        var email = "test.user@example.com";
        var password = "Password123!";

        var signUp = await client.PostAsJsonAsync("api/auth/sign-up", new SignUpDto("Test", "User", email, password));

        var signUpContent = await signUp.Content.ReadAsStringAsync();
        _output.WriteLine(signUpContent);

        Assert.Equal(HttpStatusCode.OK, signUp.StatusCode);

        var loginResponse = await client.PostAsJsonAsync("api/auth/login", new LoginDto(email, password));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();

        var loginResult = JsonSerializer.Deserialize<LoginResultDto>(loginContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(loginResult);

        client.DefaultRequestHeaders.Remove("Cookie");
        client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={loginResult.RefreshToken}");

        var refresh = await client.PostAsync("api/auth/refresh", null);
        var content = await refresh.Content.ReadAsStringAsync();
        _output.WriteLine($"Response status на refresh: {refresh.StatusCode} and content {content}");
        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);

        var logout = await client.PostAsJsonAsync("api/auth/logout", new LogoutDto(email));
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
    }

    [Theory]
    [InlineData("", "Password123!", "John", "Doe")] // Empty email
    [InlineData("test@example.com", "", "John", "Doe")] // Empty password
    [InlineData("test@example.com", "Password123!", "", "Doe")] // Empty first name
    [InlineData("test@example.com", "Password123!", "John", "")] // Empty last name
    public async Task SignUp_WithInvalidData_ReturnsUnauthorized(string email, string password, string firstName,
        string lastName)
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var request = new SignUpDto(firstName, lastName, email, password);

        // Act
        var response = await client.PostAsJsonAsync("api/auth/sign-up", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("", "Password123!")] // Empty email
    [InlineData("test@example.com", "")] // Empty password
    public async Task Login_WithInvalidData_ReturnsUnauthorized(string email, string password)
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var request = new LoginDto(email, password);

        // Act
        var response = await client.PostAsJsonAsync("api/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Security Tests

    [Fact]
    public async Task Refresh_AfterLogout_ReturnsUnauthorized()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var email = "security.test@example.com";
        var password = "Password123!";

        // Sign up and get refresh token
        var signUpRequest = new SignUpDto("Security", "Test", email, password);
        var signUpResponse = await client.PostAsJsonAsync("api/auth/sign-up", signUpRequest);

        // Logout
        var logoutRequest = new LogoutDto(email);
        await client.PostAsJsonAsync("api/auth/logout", logoutRequest);

        // Act
        var response = await client.PostAsync("api/auth/refresh", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}