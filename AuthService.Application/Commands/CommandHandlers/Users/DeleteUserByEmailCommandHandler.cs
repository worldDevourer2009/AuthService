using AuthService.Application.Services;

namespace AuthService.Application.Commands.CommandHandlers.Users;

public record DeleteUserByEmailCommand(string Email) : ICommand<DeleteUserByEmailResponse>;
public record DeleteUserByEmailResponse(bool Success, string? Message = null);

public class DeleteUserByEmailCommandHandler : ICommandHandler<DeleteUserByEmailCommand, DeleteUserByEmailResponse>
{
    private readonly IUserService _userService;

    public DeleteUserByEmailCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<DeleteUserByEmailResponse> Handle(DeleteUserByEmailCommand request, CancellationToken cancellationToken)
    {
        var result = await _userService.DeleteUserByEmail(request.Email, cancellationToken);

        if (!result)
        {
            return new DeleteUserByEmailResponse(result, $"Can't delete user with email {request.Email}");
        }
        
        return new DeleteUserByEmailResponse(result, $"User with email {request.Email} was deleted successfully");
    }
}