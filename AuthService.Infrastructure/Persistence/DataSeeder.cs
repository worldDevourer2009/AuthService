using AuthService.Application.Services;
using AuthService.Infrastructure.Interfaces;

namespace AuthService.Infrastructure.Persistence;

public class DataSeeder : IDataSeeder
{
    private readonly IApplicationDbContext _context;

    public DataSeeder(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedDataAsync()
    {
        if (!_context.Users.Any())
        {
            var list = new List<User>()
            {
                User.Create("Paul", "Walker", "hello@world123.com", "password123"),
                User.Create("Paul1", "Walker", "hello1@world123.com", "password123"),
                User.Create("Paul2", "Walker", "hello2@world123.com", "password123"),
                User.Create("Paul3", "Walker", "hello3@world123.com", "password123"),
                User.Create("Paul4", "Walker", "hello4@world123.com", "password123"),
            };
            
            await _context.Users.AddRangeAsync(list);
            
            await _context.SaveChangesAsync();
        }
    }
}