using Fintech.Api.Data;
using Fintech.Api.DTOs;
using Fintech.Api.Models;

namespace Fintech.Api.Services.Transactions;
public class DepositStrategy : ITransactionStrategy
{
    public TransactionType Type => TransactionType.Deposit;
    private readonly IAccountService _accountService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DepositStrategy> _logger;
    public DepositStrategy(IAccountService accountService, AppDbContext dbContext, ILogger<DepositStrategy> logger)
    {
        _accountService = accountService;
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<Transaction?> CreateTransactionAsync(CreateTransactionRequest transaction, Guid currentUserId)
    {
        _logger.LogInformation("Deposit transaction requested.");

        if(transaction.Amount < 0)
        {
            _logger.LogError("Transfer transaction failed. Transaction amount cannot be negative.");
            return null;
        }

        Guid? accountId = transaction.DestinationAccountId;
        if(accountId == null) return null;

        var account = await _accountService.GetAccountByIdAsync(accountId.Value);
        if(account == null)
        {
            _logger.LogError("Deposit transaction failed. Account not found.");
            return null;
        }

        if(account.IsActive == false)
        {
            _logger.LogError("Deposit transaction failed. Account is not active.");
            return null;
        }

        account!.Balance += transaction.Amount;
        
        var newTransaction = new Transaction
        {
            UserId = currentUserId,
            DestinationAccountId = accountId.Value,
            Amount = transaction.Amount,
            Description = transaction.Description,
            Type = transaction.Type
        };

        _dbContext.Transactions.Add(newTransaction);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Deposit transaction created and account balance updated.");
        return newTransaction;
    }
}