using AuthService.Application.Services;

namespace AuthService.Application.Commands.CommandHandlers.Users;

public record DeleteUserByIdCommand(Guid Id) : ICommand<DeleteUserByIdResponse>;
public record DeleteUserByIdResponse(bool Success, string? Message = null);

public class DeleteUserByIdCommandHandler : ICommandHandler<DeleteUserByIdCommand, DeleteUserByIdResponse>
{
    private readonly IUserService _userService;

    public DeleteUserByIdCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<DeleteUserByIdResponse> Handle(DeleteUserByIdCommand request, CancellationToken cancellationToken)
    {
        var result = await _userService.DeleteUserBydId(request.Id, cancellationToken);

        if (!result)
        {
            return new DeleteUserByIdResponse(result, "Can't delete user");
        }

        return new DeleteUserByIdResponse(result, "User deleted successfully");
    }
}