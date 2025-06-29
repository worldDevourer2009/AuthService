using System.Security.Cryptography;
using AuthService.Application.Options;
using AuthService.Domain.Services.Tokens;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure.Services.Tokens;

public class RSAKeyGen : IKeyGenerator, IDisposable
{
    public RSA Rsa => _rsa;
    public byte[] PublicKey => _publicKey!;
    public byte[] PrivateKey => _privateKey!;

    private readonly RSA _rsa;
    private readonly RsaKeySettings _rsaKeySettings;
    private readonly byte[]? _publicKey;
    private readonly byte[]? _privateKey;
    
    private readonly ILogger<RSAKeyGen> _logger;

    public RSAKeyGen(IHostEnvironment env, IOptions<RsaKeySettings> settings, ILogger<RSAKeyGen> logger)
    {
        _logger = logger;
        _rsaKeySettings = settings.Value;
        _rsa = RSA.Create();
        
        try
        {
            var path = Path.Combine(env.ContentRootPath, _rsaKeySettings.KeyPath);
            
            if (!File.Exists(path))
            {
                _logger.LogWarning("Rsa file not found");
                GenerateKey();
            }
            else
            {
                var pemContent = File.ReadAllText(path);
                _rsa.ImportFromPem(pemContent);
                return;
            }
            
            _publicKey = _rsa.ExportRSAPublicKey();
            _privateKey = _rsa.ExportRSAPrivateKey();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error generating RSA keys");
            GenerateKey();
            _publicKey = _rsa.ExportRSAPublicKey();
            _privateKey = _rsa.ExportRSAPrivateKey();
        }
    }
    
    public string? ExportPublicKeyPem()
    {
        if (_rsa == null)
        {
            throw new InvalidOperationException("RSA key not initialized");
        }

        var publicKeyBytes = _rsa.ExportSubjectPublicKeyInfo();
        return PemEncoding.Write("PUBLIC KEY", publicKeyBytes).ToString();
    }

    private void GenerateKey()
    {
        _rsa.KeySize = _rsaKeySettings.KeySize;
        _logger.LogInformation("Generated new RSA key");
    }

    public void Dispose()
    {
        _rsa?.Dispose();
    }
}