using Fintech.Api.Data;
using Fintech.Api.DTOs;
using Fintech.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Fintech.Api.Services;
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

    public async Task<Account> CreateAccountAsync(Guid userId,CreateAccountRequest request)
    {
        _logger.LogInformation("Trying to create account.");
        var newAccount = new Account
        {
            Name = request.Name,
            UserId = userId,
            Currency = request.Currency,
            Balance = 0
        };
        var accountCreated = await _dbContext.Accounts.AddAsync(newAccount);
        _dbContext.SaveChanges();
        _logger.LogInformation("Account with id {id} created.", accountCreated.Entity.Id);
        return accountCreated.Entity;
    }

    public async Task<Account?> UpdateAccountAsync(Guid id, UpdateAccountRequest request)
    {
        _logger.LogInformation("Account {id} requested to be updated.", id);
        var accountToUpdate = await GetAccountByIdAsync(id);
        if(accountToUpdate == null)
        {
            _logger.LogError("Account {id} not found.", id);
            return null;
        }
        if(request.Balance != null) accountToUpdate.Balance = request.Balance.Value;
        if(request.Name != null) accountToUpdate.Name = request.Name;
        if(request.IsActive != null) accountToUpdate.IsActive = request.IsActive.Value;

        _dbContext.SaveChanges();
        _logger.LogInformation("Account {id} updated.", id);
        return accountToUpdate;
    }
}