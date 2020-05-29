using Microsoft.EntityFrameworkCore;

namespace BoardIdentityServer.Persistence
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions options): base(options)
        {
        }

        public DbSet<User>? Users { get; set; } = null!;
    }
}
