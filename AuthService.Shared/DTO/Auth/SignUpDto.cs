namespace AuthService.Shared.DTO.Auth;

public record SignUpDto(
    string FirstName, 
    string LastName, 
    string Email, 
    string Password);