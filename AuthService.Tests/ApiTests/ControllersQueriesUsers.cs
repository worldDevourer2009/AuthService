using System.Net.Http.Json;
using AuthService.Infrastructure.Persistence;
using AuthService.Shared.DTO.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace AuthService.Tests.ApiTests;

public class ControllersQueriesUsers : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
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
    }

    [Fact]
    private async Task Auth_User_Sign_Up_Returns_200()
    {
        //Arrange
        var client = _webApplicationFactory.CreateClient();
        var request = new SignUpDto("TestName", "TestSurname", "TestEmail@gmail.com", "12345Db");
        
        //Act
        var response = await client.PostAsJsonAsync("api/auth/sign-up", request);
        
        //Assert
        Assert.Equal(200, (int)response.StatusCode);
    }

    [Fact]
    private async Task Auth_User_Login_Returns_200()
    {
        //Arrange
        var client = _webApplicationFactory.CreateClient();
        var signUpRequest = new SignUpDto("Paul", "Walker", "hello@world123.com", "password123");
        var request = new LoginDto("hello@world123.com", "password123");
        
        //Setup
        var signUpResponse = await client.PostAsJsonAsync("api/auth/sign-up", signUpRequest);
        
        //Assert
        Assert.Equal(200, (int)signUpResponse.StatusCode);
        
        //Act
        var response = await client.PostAsJsonAsync("api/auth/login", request);
        
        //Assert
        Assert.Equal(200, (int)response.StatusCode);
    }
}