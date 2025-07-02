namespace AuthService.Application.Options;

public class KafkaSettings
{
    public string? BootstrapServers {get; set;}
    public string? ClientId {get; set;}
    public int? MessageTimeoutMs {get; set;}
    public string GroupId { get; set; }
}