namespace AuthService.Shared.DTO.Auth.AuthResults;

public record LoginResultDto(bool Success, string? AccessToken, string? Message);