using AuthService.Domain.Entries;
using AuthService.Domain.Responses;

namespace AuthService.Domain.Services.Users;

public interface IUserSignUpService
{
    Task<SignUpResponse> SignUpAsync(SignUpEntry entry, CancellationToken cancellationToken = default);
}