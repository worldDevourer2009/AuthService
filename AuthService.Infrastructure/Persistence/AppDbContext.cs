using AuthService.Infrastructure.Interfaces;

namespace AuthService.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public DbSet<User> Users { get; private set; } = null!;

    private readonly DbContextOptions<AppDbContext> _options;
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        _options = options;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            
            entity.OwnsOne(e => e.UserIdentity, ui =>
            {
                ui.Property(e => e.Id)
                    .HasColumnName("id")
                    .IsRequired();
                
                ui.Property(e => e.FirstName)
                    .HasColumnName("first_name");
                
                ui.Property(e => e.LastName)
                    .HasColumnName("last_name");

                ui.WithOwner();
            });

            entity.Property<Guid>("id");
            
            entity.HasKey("id");
            
            entity.Property(e => e.Email)
                .HasConversion(
                    email => email.EmailAddress, 
                    emailAddress => Email.Create(emailAddress))
                .HasColumnName("email")
                .IsRequired();
            
            entity.Property(e => e.Password)
                .HasConversion(
                    password => password.PasswordHash, 
                    hash => Password.Create(hash))
                .HasColumnName("password_hash")
                .IsRequired();
            
            entity.Property(e => e.IpAddress)
                .HasConversion<string>()
                .HasColumnName("ip_address");
            
            entity.Property(e => e.LastLogin)
                .HasColumnName("last_login");
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
            
            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .IsRequired();
        });
    }
}