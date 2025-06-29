namespace AuthService.Domain.Services.Passwords;

public class PasswordHasher
{
    public static PasswordHasher Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PasswordHasher();
            }

            return _instance;
        }
    }
    
    private static PasswordHasher _instance;

    public string HashPassword(string password)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        return hashedPassword;
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
        {
            return false;
        }
        
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}