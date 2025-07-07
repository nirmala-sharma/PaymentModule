using Microsoft.AspNetCore.Identity;
using PaymentGatewayApp.Server.DatabaseContext;
using PaymentGatewayApp.Server.Interfaces;
using PaymentGatewayApp.Server.Model;

namespace PaymentGatewayApp.Server.Services
{
    public class SeedServices : ISeedService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        public SeedServices(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }
        public async Task SeedAsync()
        {
            var isUserExist = _context.Users.Any();
            if (!isUserExist)
            {
                await _context.Users.AddRangeAsync(
                    new User
                    {
                        UserId = Guid.NewGuid(),
                        UserName = "user",
                        PasswordHash = _passwordHasher.GenerateHashPassword("user123"),
                        CreatedOn = DateTime.UtcNow
                    },
                    new User
                    {
                        UserId = Guid.NewGuid(),
                        UserName = "admin",
                        PasswordHash = _passwordHasher.GenerateHashPassword("admin123"),
                        CreatedOn = DateTime.UtcNow
                    });
                await _context.SaveChangesAsync();
            }
        }
    }
}
