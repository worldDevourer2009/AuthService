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

        try
        {
            var response = await _userLoginService.LoginAsync(loginEntry, cancellationToken);

            if (!response.Success)
            {
                return new LoginResponse(false, Message: $"{response.Message}");
            }
            return new LoginResponse(true, response.AccessToken, response.RefreshToken, Message: "Login successfully");
        }
        catch (Exception ex)
        {
            return new LoginResponse(false, Message:"Caught exception {" + ex.Message + "}");
        }
    }
}