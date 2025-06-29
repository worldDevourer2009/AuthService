namespace AuthService.Application.Options;

public class RedisSettings
{
    public string? ConnectionString { get; set; }
    public string? Uri { get; set; }
    public string? InstanceName { get; set; }
    public int Port { get; set; }
}