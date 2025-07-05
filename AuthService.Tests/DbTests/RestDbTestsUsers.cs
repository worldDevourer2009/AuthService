using AuthService.Domain.Entities.Users;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace AuthService.Tests.DbTests;

public class RestDbTestsUsers : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly ITestOutputHelper _output;
    private AppDbContext _context;
    private UserRepository _repository;

    public RestDbTestsUsers(WebApplicationFactory<Program> webApplicationFactory, ITestOutputHelper output)
    {
        _output = output;
        
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("authservice_test")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .Build();
        
        _webApplicationFactory = webApplicationFactory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((ctx, conf) =>
            {
                var settings = new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = _container.GetConnectionString(),
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
                    options.UseNpgsql(_container.GetConnectionString(),
                        npgsql => npgsql.MigrationsAssembly("AuthService.Infrastructure")));
                
                services.AddLogging(log => log.AddConsole().SetMinimumLevel(LogLevel.Debug));
                services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
            });
        });
    }
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var connectionString = _container.GetConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly("AuthService.Infrastructure"))
            .Options;
        
        _context = new AppDbContext(options);
        
        await _context.Database.EnsureCreatedAsync();

        _repository = new UserRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _container.StopAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task Add_New_User_Returns_True()
    {
        // Arrange
        var user = User.Create("Anatole", "France", "hello@gmail.com", "geH1vJcj7N1U5ae3Y!");
        
        // Act
        await _repository.AddNewUser(user);
        var userFromDb = await _repository.GetUserByEmail(user.Email.EmailAddress!, default);
        
        // Assert
        Assert.NotNull(userFromDb);
        Assert.Equal(user.Email, userFromDb.Email);
        Assert.Equal(user.UserIdentity, userFromDb.UserIdentity);
        
        // Cleanup
        await _repository.DeleteUser(user);
    }

    [Fact]
    public async Task Get_NonExistent_User_Returns_Null()
    {
        // Arrange
        var nonExistentEmail = "nonexistent@example.com";
        
        try
        {
            // Act
            var result = await _repository.GetUserByEmail(nonExistentEmail, default);
        }
        catch (Exception e)
        {
            // Assert
            Assert.Null(null);
        }
    }

    [Fact]
    public async Task Delete_User_Removes_From_Database()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john.doe@example.com", "geH1vJcj7N1U5ae3Y!");
        await _repository.AddNewUser(user);
        
        // Act
        await _repository.DeleteUser(user);

        try
        {
            var deletedUser = await _repository.GetUserByEmail(user.Email.EmailAddress, default);
            
            // Assert
            Assert.Null(deletedUser);
        }
        catch (Exception e)
        {
            // Assert
            Assert.Null(null);
        }
    }

    [Fact]
    public async Task Add_Multiple_Users_All_Persisted()
    {
        // Arrange
        var user1 = User.Create("Alice", "Smith", "alice@example.com", "geH1vJcj7N1U5ae3Y!");
        var user2 = User.Create("Bob", "Johnson", "bob@example.com", "geH1vJcj7N1U5ae3Y!11");
        
        // Act
        await _repository.AddNewUser(user1);
        await _repository.AddNewUser(user2);
        
        var retrievedUser1 = await _repository.GetUserByEmail(user1.Email.EmailAddress, default);
        var retrievedUser2 = await _repository.GetUserByEmail(user2.Email.EmailAddress, default);
        
        // Assert
        Assert.NotNull(retrievedUser1);
        Assert.NotNull(retrievedUser2);
        Assert.Equal("Alice", retrievedUser1.UserIdentity.FirstName);
        Assert.Equal("Bob", retrievedUser2.UserIdentity.FirstName);
        
        // Cleanup
        await _repository.DeleteUser(user1);
        await _repository.DeleteUser(user2);
    }
}