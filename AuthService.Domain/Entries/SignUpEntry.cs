namespace AuthService.Domain.Entries;

public record SignUpEntry(string? LastName, string? FirstName, string? Email, string? Password);