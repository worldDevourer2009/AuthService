using AuthService.Domain.Services.Users;

namespace AuthService.Application.Commands.CommandHandlers.Auth;

public record SignUpRequest(string FirstName, string LastName, string Email, string Password) : ICommand<SignUpResponse>;
public record SignUpResponse(bool Success, string? AccessToken = null, string? RefreshToken = null, string? Message = null);

public class SignUpCommandHandler : ICommandHandler<SignUpRequest, SignUpResponse>
{
    private readonly IUserSignUpService _userSignUpService;

    public SignUpCommandHandler(IUserSignUpService userSignUpService)
    {
        _userSignUpService = userSignUpService;
    }

    public async Task<SignUpResponse> Handle(SignUpRequest request, CancellationToken cancellationToken)
    {
        var entry = new SignUpEntry(request.FirstName, request.LastName, request.Email, request.Password);

        try
        {
            var response = await _userSignUpService.SignUpAsync(entry, cancellationToken);

            if (!response.Success)
            {
                return new SignUpResponse(false, Message: response.Message);
            }

            return new SignUpResponse(response.Success, response.AccessToken, response.RefreshToken,
                "Sign up successfully");
        }
        catch (Exception ex)
        {
            return new SignUpResponse(false, Message: $"Caught exception {ex.Message}");
        }
    }
}