using Fintech.Api.DTOs;
using Fintech.Api.Models;

namespace Fintech.Api.Services;
public interface ITransactionService
{
    public Task<Transaction?> GetTransactionByIdAsync(Guid id, Guid currentUserId);
    public Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(Guid accountId, Guid currentUserId);
    public Task<Transaction?> CreateTransactionAsync(CreateTransactionRequest transaction, Guid currentUserId);
}