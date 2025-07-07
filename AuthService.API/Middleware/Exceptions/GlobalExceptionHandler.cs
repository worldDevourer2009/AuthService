using System.Text.Json;
using AuthService.Domain.Exceptions;
using AuthService.Domain.Exceptions.Entities;
using AuthService.Domain.Exceptions.VO;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace AuthService.API.Middleware.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception,"Occured exception {Message}",exception.Message);
        var response = httpContext.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            InvalidUserIdentity userIdentityException => new ErrorResponse
            {
                Title = "User identity exception",
                Details = userIdentityException.Message,
                Status = StatusCodes.Status400BadRequest
            },
            InvalidPasswordException invalidPasswordException => new ErrorResponse
            {
                Title = "Password exception",
                Details = invalidPasswordException.Message,
                Status = StatusCodes.Status400BadRequest
            },
            InvalidEmailException invalidEmailException => new ErrorResponse
            {
                Title = "Email exception",
                Details = invalidEmailException.Message,
                Status = StatusCodes.Status400BadRequest
            },
            _ => new ErrorResponse
            {
                Title = "Internal server error",
                Status = StatusCodes.Status500InternalServerError,
                Details = "Unexpected error happened"
            }
        };
        
        await response.WriteAsync(JsonSerializer.Serialize(errorResponse), cancellationToken);
        
        return true;
    }
}