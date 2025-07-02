using AuthService.Application.Services.Repositories;
using AuthService.Domain.VO;
using AuthService.Infrastructure.Interfaces;

namespace AuthService.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository 
{
    private readonly IApplicationDbContext _context;

    public UserRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(id));
        }
        
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        return user;
    }

    public async Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        try
        {
            var emailAddress = Email.Create(email);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == emailAddress, cancellationToken);


            if (user == null)
            {
                return null;
            }

            return user;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task AddNewUser(User user, CancellationToken cancellationToken = default)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateUser(User user, CancellationToken cancellationToken = default)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == user.Email, cancellationToken);
        
        if (existingUser == null)
        {
            throw new Exception("User not found");
        }
        
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteUser(User user, CancellationToken cancellationToken = default)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        await RemoveUserFromContext(user, cancellationToken);
    }

    public async Task DeleteUserByEmail(string? email, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email.EmailAddress == email, cancellationToken);

        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        await RemoveUserFromContext(user, cancellationToken);
    }

    public async Task DeleteUserById(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(id));
        }
        
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        await RemoveUserFromContext(user, cancellationToken);
    }

    private async Task RemoveUserFromContext(User user, CancellationToken cancellationToken)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}