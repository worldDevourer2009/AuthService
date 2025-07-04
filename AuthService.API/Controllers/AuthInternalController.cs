using System.Security.Claims;
using AuthService.Application.DTOtoTransfer;
using AuthService.Application.Options;
using AuthService.Application.Queries.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthInternalController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly InternalAuth _internalAuth;
    private readonly ILogger<AuthInternalController> _logger;

    public AuthInternalController(IMediator mediator, ILogger<AuthInternalController> logger, IOptions<InternalAuth> internalAuth)
    {
        _mediator = mediator;
        _logger = logger;
        _internalAuth = internalAuth.Value;
    }

    [HttpPost("auth-internal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateServiceToken([FromBody] Classes.GenerateInternalTokenDto dto)
    {
        if (dto.ServiceClientId != _internalAuth.ServiceClientId ||
            dto.ClientSecret != _internalAuth.ServiceClientSecret)
        {
            return Unauthorized(new Classes.AuthInternalDtoResult(false, null, "Invalid creds"));
        }
        
        var claims = new List<Claim>
        {
            new Claim("scope", "internal_api"),
            new Claim("service_name", dto.ServiceClientId)
        };

        if (dto.AdditionalClaims != null)
        {
            claims.AddRange(dto.AdditionalClaims);
        }

        var query = new InternalAuthQuery(claims);
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            _logger.LogWarning("Can't generate token {ResultMessage}", result.Message);
            return BadRequest(new Classes.AuthInternalDtoResult(result.Success, result.Token, result.Message));
        }

        _logger.LogInformation("Token generated successfully {Message}", result.Message);
        return Ok(new Classes.AuthInternalDtoResult(result.Success, result.Token, result.Message));
    }
}