namespace AuthService.Domain.Responses;

public record LoginResponse(bool Success, string? AccessToken = null, string? RefreshToken = null);