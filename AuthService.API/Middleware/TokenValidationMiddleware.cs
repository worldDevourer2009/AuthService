using System.IdentityModel.Tokens.Jwt;
using AuthService.Domain.Entities.Users;
using AuthService.Domain.Services.Tokens;
using Microsoft.AspNetCore.Http;

namespace AuthService.API.Middleware;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITokenService _tokenService;

    public TokenValidationMiddleware(RequestDelegate next, ITokenService tokenService)
    {
        _next = next;
        _tokenService = tokenService;
    }

    public async Task Invoke(HttpContext context)
    {
        var authAccessToken = context.Request.Cookies["accessToken"];
        var authRefreshToken = context.Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(authRefreshToken) && string.IsNullOrWhiteSpace(authAccessToken))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await _next.Invoke(context);
        }
        else
        {
            var isAccessTokenRevokedForUser = await _tokenService.IsAccessTokenRevokedForUser(authAccessToken!);
            var isRefreshTokenRevokedForUser = false;
            
            if (isAccessTokenRevokedForUser)
            {
                isRefreshTokenRevokedForUser = await _tokenService.IsRefreshTokenRevokedForUser(authRefreshToken!);
                
                if (isRefreshTokenRevokedForUser)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
                else
                {
                    var emailClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)
                                     ?? context.User.FindFirst(JwtRegisteredClaimNames.Email);

                    if (emailClaim == null || string.IsNullOrWhiteSpace(emailClaim.Value))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Failed to generate token");
                        return;
                    }
                    
                    var newAccessToken = await _tokenService.GenerateAccessTokenForUser(emailClaim.Value);
                    
                    var accessCookiesOptions = new CookieOptions()
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddMinutes(30)
                    };
                    
                    context.Response.Cookies.Append("accessToken", newAccessToken, accessCookiesOptions);
                }

                await _next.Invoke(context);
            }
        }
    }
}