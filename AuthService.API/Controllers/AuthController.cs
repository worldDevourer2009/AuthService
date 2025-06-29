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

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
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
            return Unauthorized(new {result = response.Success, message = response.Message});
        }

        SetRefreshTokenCookie(response);

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
            return Unauthorized(new {result = response.Success, message = response.Message});
        }
        
        SetRefreshTokenCookie(response);

        return Ok(new LoginResultDto(true, response.AccessToken, "Login Successfully"));
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
            return Unauthorized(new {result = response.Success, message = response.Message});
        }

        ClearCookies();
        
        return Ok(new {result = response.Success, message = response.Message});
    }

    private void ClearCookies()
    {
        var cookiesOptions = new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(-1)
        };
        
        Response.Cookies.Append("refreshToken", "", cookiesOptions);
        Response.Cookies.Append("accessToken", "", cookiesOptions);
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
            return Unauthorized();
        }
        
        var accessCookiesOptions = new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(30)
        };
        
        var refreshCookiesOptions = new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        
        Response.Cookies.Append("refreshToken", response.RefreshToken!, refreshCookiesOptions);
        Response.Cookies.Append("accessToken", response.AccessToken!, accessCookiesOptions);
        
        return Ok(response);
    }

    private void SetRefreshTokenCookie(LoginResponse response)
    {
        var refreshCookiesOptions = new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        
        var accessCookiesOptions = new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(30)
        };

        Response.Cookies.Append("refreshToken", response.RefreshToken!, refreshCookiesOptions);
        Response.Cookies.Append("accessToken", response.AccessToken!, accessCookiesOptions);
    }

    private void SetRefreshTokenCookie(SignUpResponse response)
    {
        var refreshCookiesOptions = new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        
        var accessCookiesOptions = new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(30)
        };

        Response.Cookies.Append("refreshToken", response.RefreshToken!, refreshCookiesOptions);
        Response.Cookies.Append("accessToken", response.AccessToken!, accessCookiesOptions);
    }
}