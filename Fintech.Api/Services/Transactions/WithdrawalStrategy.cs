using Fintech.Api.Data;
using Fintech.Api.DTOs;
using Fintech.Api.Models;

namespace Fintech.Api.Services.Transactions;
public class WithdrawalStrategy : ITransactionStrategy
{
    public TransactionType Type => TransactionType.Withdrawal;
    private readonly IAccountService _accountService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<WithdrawalStrategy> _logger;
    public WithdrawalStrategy(IAccountService accountService, AppDbContext dbContext, ILogger<WithdrawalStrategy> logger)
    {
        _accountService = accountService;
        _logger = logger;
        _dbContext = dbContext;
    }
    public async Task<Transaction?> CreateTransactionAsync(CreateTransactionRequest transaction)
    {
        _logger.LogInformation("Withdrawal transaction requested.");
        Guid? accountId = transaction.SourceAccountId;
        if(accountId == null) return null;

        var account = await _accountService.GetAccountByIdAsync(accountId.Value);
        if(account == null)
        {
            _logger.LogError("Withdrawal transaction failed. Account not found.");
            return null;
        }
        if(account!.Balance < transaction.Amount) 
        {
            _logger.LogError("Withdrawal transaction failed. Account balance is not enough.");
            return null;
        }
        
        account!.Balance -= transaction.Amount;

        var newTransaction = new Transaction
        {
            SourceAccountId = accountId.Value,
            Amount = transaction.Amount,
            Description = transaction.Description,
            Type = transaction.Type
        };

        _dbContext.Transactions.Add(newTransaction);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Withdrawal transaction created and account balance updated.");
        return newTransaction;
    }
}