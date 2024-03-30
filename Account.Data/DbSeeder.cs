using Account.Data.Entities;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Account.Data;

public interface IDbSeeder
{
    void Seed();
}

public class DbSeeder : IDbSeeder
{
    private readonly DataContext _context;

    public DbSeeder(DataContext context)
    {
        _context = context;
    }

    public void Seed()
    {
        _context.Database.EnsureCreated();

        SeedRoles();
        SeedAccounts();

    }

    private void SeedAccounts()
    {
        var account = _context.Accounts.FirstOrDefault(account => account.Email == "bob@blue.com");
        if (account == null)
        {
            _context.Accounts.Add(new UserAccount()
            {
                FirstName = "Bob",
                LastName = "Blue",
                Email = "bob@blue.com",
                AcceptTerms = true,
                PasswordHash = BCryptNet.HashPassword("12345678"),
                Verified = DateTime.UtcNow,
                Created = DateTime.UtcNow,
                Roles = _context.Roles.Where(role => role.Name.ToLower() == "admin").ToList()
            });

            _context.SaveChanges();
        }
    }

    private void SeedRoles()
    {
        var role = _context.Roles.FirstOrDefault(role => role.Name.ToLower() == "admin");
        if (role == null)
        {
            _context.Roles.Add(new Role()
            {
                Name = "Admin",
                Description = "Full permission"
            });

            _context.SaveChanges();
        }
    }
}
