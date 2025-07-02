using AuthService.Domain.Entities.Users;

namespace AuthService.Tests;

public class UnitTest1
{
    public UnitTest1()
    {
    }
    
    [Fact]
    public void Users_Emails_Equal_Returns_True()
    {
        // Arrange
        var passwordHashed = BCrypt.Net.BCrypt.HashPassword("qwerty12345");
        var passwordHashed2 = BCrypt.Net.BCrypt.HashPassword("qwerty12345");
        
        var user1 = User.Create("John", "Doe", "hello_world@gmail.com", passwordHashed);
        var user2 = User.Create("Boris", "Doe", "hello_world@gmail.com", passwordHashed2);
        
        //Setup
        Assert.Equal(user1.Email, user2.Email);
    }

    [Fact]
    public void Users_Ids_Not_Equal_Returns_False()
    {
        // Arrange
        var passwordHashed = BCrypt.Net.BCrypt.HashPassword("qwerty12345");
        var passwordHashed2 = BCrypt.Net.BCrypt.HashPassword("qwerty12345");
        
        var user1 = User.Create("John", "Doe", "hello_world@gmail.com", passwordHashed);
        var user2 = User.Create("Boris", "Doe", "hello_world_2@gmail.com", passwordHashed2);
        
        //Setup
        Assert.NotEqual(user1.Id, user2.Id);
    }
    
    [Fact]
    public void Users_With_Same_Password_Returns_False()
    {
        // Arrange
        var passwordHashed = BCrypt.Net.BCrypt.HashPassword("qwerty12345");
        var passwordHashed2 = BCrypt.Net.BCrypt.HashPassword("qwerty12345");
        
        var user1 = User.Create("John", "Doe", "hello_world@gmail.com", passwordHashed);
        var user2 = User.Create("Boris", "Doe", "hello_world_2@gmail.com", passwordHashed2);
        
        //Setup
        Assert.NotEqual(user1.Password, user2.Password);
    }
}