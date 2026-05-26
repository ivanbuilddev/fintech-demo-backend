using Fintech.Api.Data;
using Fintech.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Fintech.Api.Workers;

public class DatabaseCapWorker : BackgroundService
{
    private readonly ILogger<DatabaseCapWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private const int MaxAccounts = 10;
    private const int MaxTransactions = 100;

    public DatabaseCapWorker(ILogger<DatabaseCapWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
                var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                
                await EnforceAccountCapAsync(db, accountService);
                await EnforceTransactionCapAsync(db, transactionService);
                
                _logger.LogInformation("Database cap worker ran successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database cap worker failed.");
            }
        }
    }

    private async Task EnforceAccountCapAsync(AppDbContext db, IAccountService accountService)
    {
        var totalAccounts = await db.Accounts.CountAsync();
        if (totalAccounts <= MaxAccounts) return;

        var excessCount = totalAccounts - MaxAccounts;

        var accountsToDelete = await db.Accounts
            .OrderBy(a => a.CreatedAtUtc)
            .Take(excessCount)
            .Select(a => a.Id)
            .ToListAsync();

        foreach (var accountId in accountsToDelete)
            await accountService.ForceDeleteAccountAsync(accountId);

        _logger.LogInformation(
            "Account cap enforced: removed {Count} accounts.",
            accountsToDelete.Count
        );
    }

    private async Task EnforceTransactionCapAsync(AppDbContext db, ITransactionService transactionService)
    {
        var totalTransactions = await db.Transactions.CountAsync();
        if (totalTransactions <= MaxTransactions) return;

        var excessCount = totalTransactions - MaxTransactions;

        var transactionIdsToDelete = await db.Transactions
            .OrderBy(t => t.CreatedAtUtc)
            .Take(excessCount)
            .Select(t => t.Id)
            .ToListAsync();

        foreach (var transactionId in transactionIdsToDelete)
            await transactionService.ForceDeleteTransactionAsync(transactionId);

        _logger.LogInformation(
            "Transaction cap enforced: removed {Count} transactions.",
            transactionIdsToDelete.Count
        );
    }
}