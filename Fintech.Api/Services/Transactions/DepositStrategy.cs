using Fintech.Api.Data;
using Fintech.Api.Models;

namespace Fintech.Api.Services.Transactions
{
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
        public async Task<Transaction?> CreateTransactionAsync(Transaction transaction)
        {
            _logger.LogInformation("Deposit transaction requested.");
            Guid? accountId = transaction.DestinationAccountId;
            if(accountId == null) return null;

            var account = await _accountService.GetAccountByIdAsync(accountId.Value);
            if(account == null)
            {
                _logger.LogError("Deposit transaction failed. Account not found.");
                return null;
            }
            account!.Balance += transaction.Amount;
            await _accountService.UpdateAccountAsync(account);
            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Deposit transaction created and account balance updated.");
            return transaction;
        }
    }
}