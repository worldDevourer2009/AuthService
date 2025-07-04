namespace AuthService.Application.Commands.CommandHandlers.Users;

public record CreateUserCommand(string FirstName, string LastName, string Email, string Password) 
    : ICommand<CreateUserResponse>;
public record CreateUserResponse(bool Success, string? Message = null);

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    public async Task<CreateUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}