using fintech_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace fintech_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<User> Users => Set<User>();
        public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region User Configuration
            
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>().Property(u => u.Role).HasConversion<string>();

            #endregion

            #region Account Configuration

            modelBuilder.Entity<Account>().Property(a => a.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            #endregion

            #region Transaction Configuration

            modelBuilder.Entity<Transaction>().Property(t => t.Type).HasConversion<string>();
            modelBuilder.Entity<Transaction>().HasOne<Account>().WithMany().HasForeignKey(t => t.SourceAccountId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Transaction>().HasOne<Account>().WithMany().HasForeignKey(t => t.DestinationAccountId).OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Portfolio Demo Data Seeding

            var adminUserId = Guid.Parse("00000000-0000-0000-0000-000000000000");
            var aliceUserId = Guid.Parse("a1111111-1111-1111-1111-111111111111");
            var bobUserId = Guid.Parse("b2222222-2222-2222-2222-222222222222");
            
            var aliceAccountId = Guid.Parse("a3333333-3333-3333-3333-333333333333");
            var bobAccountId = Guid.Parse("b4444444-4444-4444-4444-444444444444");

            modelBuilder.Entity<User>().HasData(
                new User { Id = adminUserId, Username = "AdminDemo", Role = UserRoles.Admin, CreatedAtUtc = DateTime.UnixEpoch },
                new User { Id = aliceUserId, Username = "AliceDemo", Role = UserRoles.User, CreatedAtUtc = DateTime.UnixEpoch },
                new User { Id = bobUserId, Username = "BobDemo", Role = UserRoles.User, CreatedAtUtc = DateTime.UnixEpoch }
            );

            modelBuilder.Entity<Account>().HasData(
                new Account { Id = aliceAccountId, UserId = aliceUserId, Balance = 1000.00m, Currency = "EUR" },
                new Account { Id = bobAccountId, UserId = bobUserId, Balance = 500.00m, Currency = "EUR" }
            );

            #endregion
        }
    }
}