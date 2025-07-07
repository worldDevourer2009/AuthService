using System.Net.Http.Json;
using AuthService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using TaskHandler.Shared.Auth.DTO.Auth;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit.Abstractions;

namespace AuthService.Tests.ApiTests;

public class ControllersQueriesUsers : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private readonly RedisContainer _redisContainer;
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly ITestOutputHelper _output;
    private AppDbContext _context;

    public ControllersQueriesUsers(WebApplicationFactory<Program> webApplicationFactory, ITestOutputHelper output)
    {
        _output = output;
        
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("authservice_test_api")
            .WithUsername("testuser_api")
            .WithPassword("testpassword_api")
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
                    ["ConnectionStrings:Redis"] = $"{_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort(6379)}",
                    ["JwtSettings:Issuer"] = "test-issuer",
                    ["JwtSettings:Audience"] = "test-audience",
                    ["JwtSettings:Key"] = "Nj8s@Z%~4vH8*91@",
                    ["RsaKeySettings:KeyPath"] = "Keys/key.pem",
                    ["RsaKeySettings:GenerateIfMissing"] = "true",
                    ["RsaKeySettings:KeySize"] = "2048"
                };
                conf.AddInMemoryCollection(settings!);
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
        await _context.DisposeAsync();
        await _webApplicationFactory.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
    }

    [Fact]
    private async Task Auth_User_Sign_Up_Returns_200()
    {
        //Arrange
        var client = _webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions
            { HandleCookies = false });
        var request = new SignUpDto("TestName", "TestSurname", "TestEmail@gmail.com", "geH1vJcj7N1U5ae3Y!");
        
        //Act
        var response = await client.PostAsJsonAsync("api/auth/sign-up", request);
        var signUpContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(signUpContent);
        
        //Assert
        Assert.Equal(200, (int)response.StatusCode);
    }

    [Fact]
    private async Task Auth_User_Login_Returns_200()
    {
        //Arrange
        var client = _webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions
            { HandleCookies = false });
        var signUpRequest = new SignUpDto("Paul", "Walker", "hello@world123.com", "geH1vJcj7N1U5ae3Y!");
        
        //Setup
        var signUpResponse = await client.PostAsJsonAsync("api/auth/sign-up", signUpRequest);
        
        var signUpContent = await signUpResponse.Content.ReadAsStringAsync();
        _output.WriteLine(signUpContent);
        
        //Assert
        Assert.Equal(200, (int)signUpResponse.StatusCode);
        
        // //Act
        // var response = await client.PostAsJsonAsync("api/auth/login", request);
        //
        // //Assert
        // Assert.Equal(200, (int)response.StatusCode);
    }
}