using AuthService.Domain.Entities.Users;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Persistence.Repositories;
using AuthService.Infrastructure.Services.Passwords;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace AuthService.Tests.PasswordsTests;

public class PasswordTestInternal : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private PasswordService _passwordService;
    private AppDbContext _dbContext;
    private UserRepository _userRepository;

    public PasswordTestInternal()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("authservice_test")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var connectionString = _container.GetConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("AuthService.Infrastructure");
        })
            .Options;
        
        _dbContext = new AppDbContext(options);
        
        await _dbContext.Database.EnsureCreatedAsync();
        
        _userRepository = new UserRepository(_dbContext);
        _passwordService = new PasswordService(_userRepository);
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }

    [Fact]
    private async Task Verify_Password_Returns_True()
    {
        //Arrange
        var newUser = User.Create("TestName", "TestSurname", "test-email@gmail.com", "123456Db");
        await _userRepository.AddNewUser(newUser);
        
        //Act
        var result = await _passwordService.VerifyPasswordForUser("123456Db", newUser.Password.PasswordHash!);
        
        //Assert
        Assert.True(result);
    }

    [Fact]
    private async Task Verify_Password_Returns_False()
    {
        //Arrange
        var newUser = User.Create("TestName", "TestSurname", "test-email@gmail.com", "123456Db");
        await _userRepository.AddNewUser(newUser);
        
        //Act
        var result = await _passwordService.VerifyPasswordForUser("wrongPassword", newUser.Password.PasswordHash!);
        
        //Assert
        Assert.False(result);
    }

    [Fact]
    private async Task Verify_Password_With_Wrong_Email()
    {
        //Arrange
        var newUser = User.Create("TestName", "TestSurname", "test-email@gmail.com", "123456Db");
        await _userRepository.AddNewUser(newUser);

        bool result = false;

        try
        {
            //Act
            result = await _passwordService.VerifyPasswordForUser("wrongEmail", "123456Db");
        }
        catch (Exception e)
        {
            //Assert
            Assert.False(result);
        }
    }
}