using AuthService.Domain.Services.Users;

namespace AuthService.Application.Commands.CommandHandlers.Auth;

public record LogoutRequest(string? Email) : ICommand<LogoutResponse>;
public record LogoutResponse(bool Success, string? Message = null);

public class LogoutCommandHandler : ICommandHandler<LogoutRequest, LogoutResponse>
{
    private readonly IUserLogoutService _userLogoutService;

    public LogoutCommandHandler(IUserLogoutService userLogoutService)
    {
        _userLogoutService = userLogoutService;
    }

    public async Task<LogoutResponse> Handle(LogoutRequest request, CancellationToken cancellationToken)
    {
        var response = await _userLogoutService.LogoutAsync(request.Email, cancellationToken);
        return new LogoutResponse(response.Success, response.Message);
    }
}