namespace AuthService.Shared.DTO.Auth.AuthResults;

public record SignUpResultDto(bool Success, string? AccessToken = null, string? Message = null);