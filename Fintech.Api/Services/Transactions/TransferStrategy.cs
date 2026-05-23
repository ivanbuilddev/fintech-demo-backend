using Fintech.Api.Data;
using Fintech.Api.DTOs;
using Fintech.Api.Models;

namespace Fintech.Api.Services.Transactions
{
    public class TransferStrategy : ITransactionStrategy
    {
        public TransactionType Type => TransactionType.Deposit;
        private readonly IAccountService _accountService;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<TransferStrategy> _logger;
        public TransferStrategy(IAccountService accountService, AppDbContext dbContext, ILogger<TransferStrategy> logger)
        {
            _accountService = accountService;
            _logger = logger;
            _dbContext = dbContext;
        }
        public async Task<Transaction?> CreateTransactionAsync(CreateTransationRequest transaction)
        {
            _logger.LogInformation("Transfer transaction requested.");
            Guid? accountId = transaction.SourceAccountId;
            if(accountId == null) return null;
            
            Guid? destinationAccountId = transaction.DestinationAccountId;
            var destinationAccount = await _accountService.GetAccountByIdAsync(destinationAccountId!.Value);
            if(destinationAccount == null)
            {
                _logger.LogError("Transfer transaction failed. Destination account not found.");
                return null;
            }

            var account = await _accountService.GetAccountByIdAsync(accountId.Value);
            if(account == null)
            {
                _logger.LogError("Transfer transaction failed. Account not found.");
                return null;
            }

            if(account!.Balance < transaction.Amount) 
            {
                _logger.LogError("Transfer transaction failed. Account balance is not enough.");
                return null;
            }

            account!.Balance -= transaction.Amount;

            var newTransaction = new Transaction
            {
                SourceAccountId = accountId.Value,
                DestinationAccountId = destinationAccountId.Value,
                Amount = transaction.Amount,
                Description = transaction.Description,
                Type = transaction.Type
            };

            _dbContext.Transactions.Add(newTransaction);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Transfer transaction created and account balance updated.");
            return newTransaction;
        }
    }
}