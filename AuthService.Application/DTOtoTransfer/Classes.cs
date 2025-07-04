using System.Security.Claims;

namespace AuthService.Application.DTOtoTransfer;

public class Classes
{
    public record AuthInternalDtoResult(bool Success, string? Token = null, string? Message = null);

    public record GenerateInternalTokenDto(string ServiceClientId, string ClientSecret, IEnumerable<Claim>? AdditionalClaims = null);
}