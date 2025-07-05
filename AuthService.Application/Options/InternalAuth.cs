namespace AuthService.Application.Options;

public class InternalAuth
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string ServiceClientId { get; set; }
    public string ServiceClientSecret { get; set; }
    public int AccessTokenExpirationMinutes { get; set; }
}