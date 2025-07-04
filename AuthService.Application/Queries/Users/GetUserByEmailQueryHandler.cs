using AuthService.Application.Services;

namespace AuthService.Application.Queries.Users;

public record GetUserByEmailQueryRequest(string? Email) : IQuery<GetUserByEmailQueryResponse>;
public record GetUserByEmailQueryResponse(User? User, bool Success);

public class GetUserByEmailQueryHandler : IQueryHandler<GetUserByEmailQueryRequest, GetUserByEmailQueryResponse>
{
    private readonly IUserService _userService;

    public GetUserByEmailQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<GetUserByEmailQueryResponse> Handle(GetUserByEmailQueryRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new GetUserByEmailQueryResponse(null!, false);
        }
        
        var user = await _userService.GetUserByEmail(request.Email, cancellationToken);

        if (user! == null)
        {
            return new GetUserByEmailQueryResponse(null!, false);
        }
        
        return new GetUserByEmailQueryResponse(user, true);
    }
}