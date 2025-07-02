using AuthService.Domain.VO;
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
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();
            
            entity.OwnsOne(e => e.UserIdentity, ui =>
            {
                ui.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(100);
                
                ui.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(100);

                ui.WithOwner();
            });
            
            entity.Property(e => e.Email)
                .HasConversion(
                    email => email.EmailAddress, 
                    emailAddress => Email.Create(emailAddress))
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();
            
            entity.Property(e => e.Password)
                .HasConversion(
                    password => password.PasswordHash, 
                    hash => Password.FromHash(hash!))
                .HasColumnName("password_hash")
                .HasMaxLength(500)
                .IsRequired();
            
            entity.Property(e => e.IpAddress)
                .HasConversion<string>()
                .HasColumnName("ip_address")
                .HasMaxLength(45);;
            
            entity.Property(e => e.LastLogin)
                .HasColumnName("last_login");
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
            
            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .IsRequired()
                .HasDefaultValue(false);

            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
        });
    }
}