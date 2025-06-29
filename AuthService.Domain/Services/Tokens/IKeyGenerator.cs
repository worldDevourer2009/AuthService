using System.Security.Cryptography;

namespace AuthService.Domain.Services.Tokens;

public interface IKeyGenerator
{
    public RSA Rsa { get; }
    public byte[] PublicKey { get; }
    public byte[] PrivateKey { get; }
}