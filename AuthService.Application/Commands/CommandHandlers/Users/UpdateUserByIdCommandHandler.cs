using AuthService.Application.Services;

namespace AuthService.Application.Commands.CommandHandlers.Users;

public record UpdateUserByIdCommand(
    string Id,
    string? FirstName = null,
    string? LastName = null,
    string? Password = null,
    string? Email = null) : ICommand<UpdateUserResponse>;

public record UpdateUserResponse(bool Success, string? Message = null);

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserByIdCommand, UpdateUserResponse>
{
    private readonly IUserService _userService;

    public UpdateUserCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<UpdateUserResponse> Handle(UpdateUserByIdCommand request, CancellationToken cancellationToken)
    {
        bool result = false;
        
        if (Guid.TryParse(request.Id, out var id))
        {
            var user = await _userService.GetUserById(id, cancellationToken);

            if (user is null)
            {
                return new UpdateUserResponse(false, $"Can't find user with id {request.Id}");
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                user.SetNewEmail(request.Email);
            }
            
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.SetNewPassword(request.Password);
            }
            
            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                user.SetFirstName(request.FirstName);
            }
            
            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                user.SetLastName(request.LastName);
            }

            result = await _userService.UpdateUser(user, cancellationToken);
        }

        if (!result)
        {
            return new UpdateUserResponse(result, "Failed to update user");
        }
        
        return new UpdateUserResponse(result, $"User with id {request.Id} updated successfully");
    }
}