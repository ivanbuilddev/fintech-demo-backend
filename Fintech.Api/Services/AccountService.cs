using Fintech.Api.Data;
using Fintech.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Fintech.Api.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AccountService> _logger;

        public AccountService(AppDbContext dbContext, ILogger<AccountService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Account?> GetAccountByIdAsync(Guid id)
        {
            _logger.LogInformation("Account {id} requested.", id);
            var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == id);
            if(account == null)
            {
                _logger.LogError("Account {id} not found.", id);
                return null;
            }
            _logger.LogInformation("Account {id} found.", id);
            return account;
        }

        public async Task<IEnumerable<Account>> GetAccountsAsync(Guid userId)
        {
            _logger.LogInformation("Accounts requested for user {userId}.", userId);
            return await _dbContext.Accounts.Where(a => a.UserId == userId).ToListAsync();
        }

        public async Task<Account> CreateAccountAsync(Account account)
        {
            _logger.LogInformation("Trying to create account.");
            var accountCreated = await _dbContext.Accounts.AddAsync(account);
            _dbContext.SaveChanges();
            _logger.LogInformation("Account with id {id} created.", accountCreated.Entity.Id);
            return accountCreated.Entity;
        }

        public async Task<Account?> UpdateAccountAsync(Account account)
        {
            _logger.LogInformation("Account {id} requested to be updated.", account.Id);
            var accountToUpdate = await GetAccountByIdAsync(account.Id);
            if(accountToUpdate == null)
            {
                _logger.LogError("Account {id} not found.", account.Id);
                return null;
            }
            accountToUpdate = account;
            _dbContext.SaveChanges();
            _logger.LogInformation("Account {id} updated.", account.Id);
            return accountToUpdate;
        }
    }
}