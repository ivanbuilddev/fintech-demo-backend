using Fintech.Api.Models;

namespace Fintech.Api.Services
{
    public interface ITransactionService
    {
        public Task<Transaction?> GetTransactionByIdAsync(Guid id);
        public Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(Guid accountId);
        public Task<Transaction?> CreateTransactionAsync(Transaction transaction);
    }
}