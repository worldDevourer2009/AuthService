using AuthService.Domain.Services.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("/.well-known/jwks.json")]
public class KeyController : ControllerBase
{
    private readonly IKeyGenerator _keyGenerator;

    public KeyController(IKeyGenerator keyGenerator)
    {
        _keyGenerator = keyGenerator;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Get()
    {
        var publicKey = _keyGenerator.ExportPublicKeyPem();
        
        if (publicKey == null)
        {
            return NoContent();
        }
        
        return Content(publicKey, "text/plain");
    }
}