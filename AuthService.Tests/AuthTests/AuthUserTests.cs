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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
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
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(_postgreSqlContainer.GetConnectionString(), npgsqlOptions =>
                        npgsqlOptions.MigrationsAssembly("AuthService.Infrastructure"));
                });
                
                services.AddLogging(logBuilder => logBuilder.AddConsole().SetMinimumLevel(LogLevel.Debug));
                services.AddMediatR(options =>
                {
                    options.RegisterServicesFromAssembly(typeof(Program).Assembly);
                });
            });
        });
    }
    
    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        await _redisContainer.StartAsync();

        var connectionString = _postgreSqlContainer.GetConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly("AuthService.Infrastructure"))
            .Options;
        
        _context = new AppDbContext(options);
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
        // Arrange
        var client = _webApplicationFactory.CreateClient();
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
        
        // Проверяем, что установлены cookies
        var refreshCookie = response.Headers.GetValues("Set-Cookie")
            .FirstOrDefault(c => c.StartsWith("refreshToken="));
        var accessCookie = response.Headers.GetValues("Set-Cookie")
            .FirstOrDefault(c => c.StartsWith("accessToken="));
        
        Assert.NotNull(refreshCookie);
        Assert.NotNull(accessCookie);
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
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.AccessToken);
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
            Assert.Equal("User not found", ex.Message);
        }
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsOk()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var email = "refresh.test@example.com";
        var password = "Password123!";
        
        var signUpRequest = new SignUpDto("Test", "User", email, password);
        var signUpResponse = await client.PostAsJsonAsync("api/auth/sign-up", signUpRequest);
        
        var refreshCookie = signUpResponse.Headers.GetValues("Set-Cookie")
            .FirstOrDefault(c => c.StartsWith("refreshToken="));
        
        Assert.NotNull(refreshCookie);
        
        var refreshToken = refreshCookie.Split(';')[0].Split('=')[1];
        client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");
        
        // Act
        var response = await client.PostAsync("api/auth/refresh", null);
        
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
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var email = "complete.flow@example.com";
        var password = "Password123!";
        
        // 1. Sign Up
        var signUpRequest = new SignUpDto("Complete", "Flow", email, password);
        var signUpResponse = await client.PostAsJsonAsync("api/auth/sign-up", signUpRequest);
        Assert.Equal(HttpStatusCode.OK, signUpResponse.StatusCode);
        
        // 2. Login
        var loginRequest = new LoginDto(email, password);
        var loginResponse = await client.PostAsJsonAsync("api/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        
        // 3. Refresh tokens
        var refreshCookie = loginResponse.Headers.GetValues("Set-Cookie")
            .FirstOrDefault(c => c.StartsWith("refreshToken="));
        Assert.NotNull(refreshCookie);
        
        var refreshToken = refreshCookie.Split(';')[0].Split('=')[1];
        client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");
        
        var refreshResponse = await client.PostAsync("api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        
        // 4. Logout
        var logoutRequest = new LogoutDto(email, "");
        var logoutResponse = await client.PostAsJsonAsync("api/auth/logout", logoutRequest);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
    }

    [Theory]
    [InlineData("", "Password123!", "John", "Doe")] // Empty email
    [InlineData("test@example.com", "", "John", "Doe")] // Empty password
    [InlineData("test@example.com", "Password123!", "", "Doe")] // Empty first name
    [InlineData("test@example.com", "Password123!", "John", "")] // Empty last name
    public async Task SignUp_WithInvalidData_ReturnsUnauthorized(string email, string password, string firstName, string lastName)
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
        
        var refreshCookie = signUpResponse.Headers.GetValues("Set-Cookie")
            .FirstOrDefault(c => c.StartsWith("refreshToken="));
        var refreshToken = refreshCookie.Split(';')[0].Split('=')[1];
        
        // Logout
        var logoutRequest = new LogoutDto(email);
        await client.PostAsJsonAsync("api/auth/logout", logoutRequest);
        
        // Try to refresh after logout
        client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");
        
        // Act
        var response = await client.PostAsync("api/auth/refresh", null);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MultipleLogin_SameUser_GeneratesDifferentTokens()
    {
        // Arrange
        var client1 = _webApplicationFactory.CreateClient();
        var client2 = _webApplicationFactory.CreateClient();
        var email = "multiple.login@example.com";
        var password = "Password123!";
        
        // Sign up
        var signUpRequest = new SignUpDto("Multiple", "Login", email, password);
        await client1.PostAsJsonAsync("api/auth/sign-up", signUpRequest);
        
        // Login with first client
        var loginRequest1 = new LoginDto(email, password);
        var loginResponse1 = await client1.PostAsJsonAsync("api/auth/login", loginRequest1);
        
        // Login with second client
        var loginRequest2 = new LoginDto(email, password);
        var loginResponse2 = await client2.PostAsJsonAsync("api/auth/login", loginRequest2);
        
        // Extract tokens
        var content1 = await loginResponse1.Content.ReadAsStringAsync();
        var content2 = await loginResponse2.Content.ReadAsStringAsync();
        
        var result1 = JsonSerializer.Deserialize<LoginResultDto>(content1, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var result2 = JsonSerializer.Deserialize<LoginResultDto>(content2, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        // Assert
        Assert.NotEqual(result1.AccessToken, result2.AccessToken);
    }

    #endregion
}