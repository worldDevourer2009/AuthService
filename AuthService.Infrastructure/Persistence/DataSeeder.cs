using AuthService.Application.Services;
using AuthService.Domain.Services.Passwords;
using AuthService.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Persistence;

public class DataSeeder : IDataSeeder
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(IApplicationDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedDataAsync()
    {
        _logger.LogInformation("Seeding data...");
    }
}