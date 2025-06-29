using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AuthService.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../AuthService.API"))
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"), 
                b => b.MigrationsAssembly("AuthService.Infrastructure"));
        
        return new AppDbContext(optionsBuilder.Options);
    }
}