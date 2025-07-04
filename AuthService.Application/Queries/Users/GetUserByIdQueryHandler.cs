using AuthService.Application.Services;

namespace AuthService.Application.Queries.Users;

public record GetUserByIdRequest(Guid Id) : IQuery<GetUserByIdResponse>;
public record GetUserByIdResponse(User? User, bool Success);

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdRequest, GetUserByIdResponse>
{
    private readonly IUserService _userService;

    public GetUserByIdQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<GetUserByIdResponse> Handle(GetUserByIdRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserById(request.Id, cancellationToken);
        
        if (user is null)
        {
            return new GetUserByIdResponse(null, false);
        }
        
        return new GetUserByIdResponse(user, true);
    }
}