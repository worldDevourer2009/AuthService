using AuthService.Application.Commands.CommandHandlers.Auth;
using AuthService.Shared.DTO.Auth;
using AuthService.Shared.DTO.Auth.AuthResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHostEnvironment _environment;

    public AuthController(IMediator mediator, IHostEnvironment environment)
    {
        _mediator = mediator;
        _environment = environment;
    }

    [HttpPost]
    [Route("sign-up")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SignUp([FromBody] SignUpDto entry)
    {
        var request = new SignUpRequest(entry.FirstName, entry.LastName, entry.Email, entry.Password);

        var response = await _mediator.Send(request);

        if (!response.Success)
        {
            return Unauthorized(new { result = response.Success, message = response.Message });
        }

        var accessCookiesOptions = CreateCookieOptions(TimeSpan.FromMinutes(30));
        var refreshCookiesOptions = CreateCookieOptions(TimeSpan.FromDays(7));

        Response.Cookies.Append("refreshToken", response.RefreshToken!, refreshCookiesOptions);

        if (response.AccessToken != null)
        {
            Response.Cookies.Append("accessToken", response.AccessToken, accessCookiesOptions);
        }

        return Ok(new SignUpResultDto(true, response.AccessToken, "Login Successfully"));
    }

    [HttpPost]
    [Route("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto entry)
    {
        var request = new LoginRequest(entry.Email, entry.Password);

        var response = await _mediator.Send(request);

        if (!response.Success)
        {
            return Unauthorized(new { result = response.Success, message = response.Message });
        }

        var accessCookiesOptions = CreateCookieOptions(TimeSpan.FromMinutes(30));
        var refreshCookiesOptions = CreateCookieOptions(TimeSpan.FromDays(7));

        Response.Cookies.Append("refreshToken", response.RefreshToken!, refreshCookiesOptions);

        if (response.AccessToken != null)
        {
            Response.Cookies.Append("accessToken", response.AccessToken, accessCookiesOptions);
        }

        return Ok(new LoginResultDto(true, response.RefreshToken, response.AccessToken, "Login Successfully"));
    }

    [HttpPost]
    [Route("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] LogoutDto entry)
    {
        var request = new LogoutRequest(entry.Email);

        var response = await _mediator.Send(request);

        if (!response.Success)
        {
            return Unauthorized(new { result = response.Success, message = response.Message });
        }

        ClearCookies();

        return Ok(new { result = response.Success, message = response.Message });
    }

    [HttpPost]
    [Route("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        var request = new RefreshTokensRequest(Request.Cookies["refreshToken"]);
        var response = await _mediator.Send(request);

        if (!response.Success)
        {
            return Unauthorized($"{response.Message}");
        }

        var accessCookiesOptions = CreateCookieOptions(TimeSpan.FromMinutes(30));
        var refreshCookiesOptions = CreateCookieOptions(TimeSpan.FromDays(7));

        Response.Cookies.Append("refreshToken", response.RefreshToken!, refreshCookiesOptions);
        Response.Cookies.Append("accessToken", response.AccessToken!, accessCookiesOptions);

        return Ok(response);
    }

    private CookieOptions CreateCookieOptions(TimeSpan? expires = null)
    {
        var isProd = !_environment.IsDevelopment() && !_environment.IsEnvironment("Testing");
        
        return new CookieOptions()
        {
            HttpOnly = true,
            Secure = isProd,
            SameSite = SameSiteMode.Lax,
            Expires = expires.HasValue ? DateTime.UtcNow.Add(expires.Value) : null
        };
    }

    private void ClearCookies()
    {
        var isProd = !_environment.IsDevelopment() && !_environment.IsEnvironment("Testing");
        
        var cookiesOptions = new CookieOptions()
        {
            HttpOnly = true,
            Secure = isProd,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(-1)
        };

        Response.Cookies.Append("refreshToken", "", cookiesOptions);
        Response.Cookies.Append("accessToken", "", cookiesOptions);
    }
}