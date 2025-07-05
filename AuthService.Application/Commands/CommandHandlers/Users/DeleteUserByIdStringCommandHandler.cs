using AuthService.Application.Services;

namespace AuthService.Application.Commands.CommandHandlers.Users;

public record DeleteUserByIdStringCommand(string Id) : ICommand<DeleteUserByIdStringResponse>;
public record DeleteUserByIdStringResponse(bool Success, string? Message = null);

public class DeleteUserByIdStringCommandHandler : ICommandHandler<DeleteUserByIdStringCommand, DeleteUserByIdStringResponse>
{
    private readonly IUserService _userService;

    public DeleteUserByIdStringCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<DeleteUserByIdStringResponse> Handle(DeleteUserByIdStringCommand request, CancellationToken cancellationToken)
    {
        var result = await _userService.DeleteUserBydId(request.Id, cancellationToken);

        if (!result)
        {
            return new DeleteUserByIdStringResponse(result, "Can't delete user");
        }
        
        return new DeleteUserByIdStringResponse(result, "User deleted successfully");
    }
}