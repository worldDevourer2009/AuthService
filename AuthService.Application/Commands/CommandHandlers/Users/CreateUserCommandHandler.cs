using AuthService.Application.Services;

namespace AuthService.Application.Commands.CommandHandlers.Users;

public record CreateUserCommand(string FirstName, string LastName, string Email, string Password) 
    : ICommand<CreateUserResponse>;
public record CreateUserResponse(bool Success, string? Message = null);

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IUserService _userService;

    public CreateUserCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<CreateUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = User.Create(request.FirstName, request.LastName, request.Email, request.Password);
        var result = await _userService.CreateUser(user, cancellationToken);

        if (!result)
        {
            return new CreateUserResponse(result, "Can't create user");
        }
        
        return new CreateUserResponse(result, "User created successfully");
    }
}