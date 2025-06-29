namespace AuthService.Domain.Responses;

public record SignUpResponse(bool Success, string? AccessToken = null, string? RefreshToken = null);