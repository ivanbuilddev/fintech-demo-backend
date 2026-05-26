using Fintech.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Fintech.Api.Data;

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

        var adminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var aliceUserId = Guid.Parse("a1111111-1111-1111-1111-111111111111");
        var bobUserId = Guid.Parse("b2222222-2222-2222-2222-222222222222");
        
        var aliceAccountId = Guid.Parse("a3333333-3333-3333-3333-333333333333");
        var bobAccountId = Guid.Parse("b4444444-4444-4444-4444-444444444444");

        var aliceInitialDepositTransactionId = Guid.Parse("c1111111-1111-1111-1111-111111111111");
        var bobInitialDepositTransactionId = Guid.Parse("c2222222-2222-2222-2222-222222222222");
        var aliceTransferBobTransactionId = Guid.Parse("a5555555-5555-5555-5555-555555555555");
        var aliceDepositTransactionId = Guid.Parse("b6666666-6666-6666-6666-666666666666");
        var aliceWithdrawalTransactionId = Guid.Parse("b7777777-7777-7777-7777-777777777777");

        modelBuilder.Entity<User>().HasData(
            new User { Id = adminUserId, Username = "AdminDemo", Role = UserRoles.Admin, CreatedAtUtc = DateTime.UnixEpoch },
            new User { Id = aliceUserId, Username = "AliceDemo", Role = UserRoles.User, CreatedAtUtc = DateTime.UnixEpoch },
            new User { Id = bobUserId, Username = "BobDemo", Role = UserRoles.User, CreatedAtUtc = DateTime.UnixEpoch }
        );

        modelBuilder.Entity<Account>().HasData(
            new Account { Id = aliceAccountId, Name = "Alice Account", IsActive = true, UserId = aliceUserId, Balance = 1000.00m, Currency = "EUR" },
            new Account { Id = bobAccountId, Name = "Bob Account", IsActive = true, UserId = bobUserId, Balance = 500.00m, Currency = "EUR" }
        );

        modelBuilder.Entity<Transaction>().HasData(
            new Transaction { Id = aliceTransferBobTransactionId, Category = "Dining", Description = "Transfer to Bob", UserId = aliceUserId, Type = TransactionType.Transfer, SourceAccountId = aliceAccountId, DestinationAccountId = bobAccountId, Amount = 100.00m, CreatedAtUtc = DateTime.UnixEpoch }
            , new Transaction { Id = aliceDepositTransactionId, Category = "Mortgage", Description = "Deposit to Alice", UserId = aliceUserId, Type = TransactionType.Deposit, DestinationAccountId = aliceAccountId, Amount = 100.00m, CreatedAtUtc = DateTime.UnixEpoch }
            , new Transaction { Id = aliceWithdrawalTransactionId, Category = "Gym", Description = "Withdrawal from Alice", UserId = aliceUserId, Type = TransactionType.Withdrawal, SourceAccountId = aliceAccountId, Amount = 100.00m, CreatedAtUtc = DateTime.UnixEpoch }
            , new Transaction { Id = aliceInitialDepositTransactionId, Category = "Bank", Description = "Initial Deposit to Alice", UserId = aliceUserId, Type = TransactionType.Deposit, DestinationAccountId = aliceAccountId, Amount = 1100.00m, CreatedAtUtc = DateTime.UnixEpoch }
            , new Transaction { Id = bobInitialDepositTransactionId, Category = "Bank", Description = "Initial Deposit to Bob", UserId = bobUserId, Type = TransactionType.Deposit, DestinationAccountId = bobAccountId, Amount = 400.00m, CreatedAtUtc = DateTime.UnixEpoch }
        );

        #endregion
    }
}