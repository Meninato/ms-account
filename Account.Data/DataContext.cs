using Account.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Account.Data;

public class DataContext : DbContext
{
    public DbSet<UserAccount> Accounts { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        string? connectionString = Environment.GetEnvironmentVariable("ACCOUNT_SERVICE_CS");
        if(string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException("Missing ACCOUNT_SERVICE_CS environment variable");
        }

        options.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Owned<RefreshToken>();

        modelBuilder.Entity<UserAccount>(e =>
        {
            e.HasIndex(e => e.Email).IsUnique();
            e.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            e.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            e.Property(e => e.Email).IsRequired().HasMaxLength(254);
            e.Property(e => e.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.HasIndex(e => e.Name).IsUnique();
            e.Property(e => e.Name).IsRequired().HasMaxLength(50);
            e.Property(e => e.Description).IsRequired().HasMaxLength(150);
        });
    }
}
