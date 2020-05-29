using Microsoft.EntityFrameworkCore;

namespace BoardIdentityServer.Persistence
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions options): base(options)
        {
        }

        public DbSet<User>? Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
