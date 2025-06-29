using AuthService.Domain.Services.Users;

namespace AuthService.Application.Commands.CommandHandlers.Auth;

public record LoginRequest(string Email, string Password) : ICommand<LoginResponse>;
public record LoginResponse(bool Success, string? AccessToken = null, string? RefreshToken = null, string? Message = null);

public class LoginCommandHandler : ICommandHandler<LoginRequest, LoginResponse>
{
    private readonly IUserLoginService _userLoginService;

    public LoginCommandHandler(IUserLoginService userLoginService)
    {
        _userLoginService = userLoginService;
    }

    public async Task<LoginResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var loginEntry = new LoginEntry(request.Email, request.Password);
        
        var response = await _userLoginService.LoginAsync(loginEntry, cancellationToken);

        if (string.IsNullOrWhiteSpace(response.AccessToken))
        {
            return new LoginResponse(false, Message:"Can't create token");
        }
        
        return new LoginResponse(true, response.AccessToken, response.RefreshToken, Message:"Login successfully");
    }
}