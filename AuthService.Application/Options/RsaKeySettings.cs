namespace AuthService.Application.Options;

public class RsaKeySettings
{
    public string KeyPath { get; set; } = "Keys/key.pem";
    public int KeySize { get; set; } = 2048;
    public bool GenerateIfMissing { get; set; } = true;
}