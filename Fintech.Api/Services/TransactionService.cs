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

    public TransactionService(AppDbContext dbContext, ILogger<TransactionService> logger, IEnumerable<ITransactionStrategy> strategies)
    {
        _dbContext = dbContext;
        _logger = logger;

        Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        foreach (var s in strategies)
            Console.WriteLine($"Strategy: {s.GetType().Name} - Type: {s.Type}");

        _strategies = strategies.ToDictionary(s => s.Type.ToString(), s => s);
    }
    public async Task<Transaction?> GetTransactionByIdAsync(Guid id)
    {
        _logger.LogInformation("Transaction {id} requested.", id);
        var transaction = await _dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == id);
        if(transaction == null)
        {
            _logger.LogError("Transaction {id} not found.", id);
            return null;
        }
        _logger.LogInformation("Transaction {id} found.", id);
        return transaction;
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(Guid accountId)
    {
        _logger.LogInformation("Transactions requested for account {accountId}.", accountId);
        return await _dbContext.Transactions.Where(t => t.SourceAccountId == accountId || t.DestinationAccountId == accountId).ToListAsync();
    }

    public async Task<Transaction?> CreateTransactionAsync(CreateTransactionRequest transaction)
    {
        if(!_strategies.TryGetValue(transaction.Type.ToString(), out var strategy))
        {
            _logger.LogError("Transaction type not found.");
            return null;
        }
        var transactionCreated = await strategy.CreateTransactionAsync(transaction);
        return transactionCreated;
    }
}