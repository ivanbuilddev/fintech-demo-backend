using Fintech.Api.Data;
using Fintech.Api.DTOs;
using Fintech.Api.Models;
using Fintech.Api.Services.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Fintech.Api.Services;
public class TransactionService : ITransactionService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<TransactionService> _logger;
    private readonly Dictionary<string, ITransactionStrategy> _strategies;
    private readonly IAccountService _accountService;

    public TransactionService(AppDbContext dbContext, ILogger<TransactionService> logger, IEnumerable<ITransactionStrategy> strategies, IAccountService accountService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _strategies = strategies.ToDictionary(s => s.Type.ToString(), s => s);
        _accountService = accountService;
    }
    public async Task<Transaction?> GetTransactionByIdAsync(Guid id, Guid currentUserId)
    {
        _logger.LogInformation("Transaction {id} requested.", id);
        var transaction = await _dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == id);
        if(transaction == null)
        {
            _logger.LogError("Transaction {id} not found.", id);
            return null;
        }

        if(transaction.UserId != currentUserId)
        {
            _logger.LogError("User {currentUserId} is not the owner of transaction {id}.", currentUserId, id);
            throw new UnauthorizedAccessException("User is not the owner of the transaction.");
        }

        _logger.LogInformation("Transaction {id} found.", id);
        return transaction;
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(Guid accountId, Guid currentUserId)
    {
        _logger.LogInformation("Transactions requested for account {accountId}.", accountId);
        await _accountService.GetAccountByIdAsync(accountId, currentUserId);
        return await _dbContext.Transactions.Where(t => t.SourceAccountId == accountId || t.DestinationAccountId == accountId).ToListAsync();
    }

    public async Task<Transaction?> CreateTransactionAsync(CreateTransactionRequest transaction, Guid currentUserId)
    {
        if(!_strategies.TryGetValue(transaction.Type.ToString(), out var strategy))
        {
            _logger.LogError("Transaction type not found.");
            return null;
        }
        return await strategy.CreateTransactionAsync(transaction, currentUserId);
    }
}