using AuthService.Domain.Services.Users;

namespace AuthService.Application.Commands.CommandHandlers.Auth;

public record LogoutCommand(string? Email) : ICommand<LogoutResponse>;
public record LogoutResponse(bool Success, string? Message = null);

public class LogoutCommandHandler : ICommandHandler<LogoutCommand, LogoutResponse>
{
    private readonly IUserLogoutService _userLogoutService;

    public LogoutCommandHandler(IUserLogoutService userLogoutService)
    {
        _userLogoutService = userLogoutService;
    }

    public async Task<LogoutResponse> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var response = await _userLogoutService.LogoutAsync(command.Email, cancellationToken);
        return new LogoutResponse(response.Success, response.Message);
    }
}