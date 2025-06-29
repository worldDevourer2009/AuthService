namespace AuthService.Application.Options;

public class JwtSettings
{
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public bool ValidateIssuer { get; set; }
    public bool ValidateSigningKey { get; set; }
}