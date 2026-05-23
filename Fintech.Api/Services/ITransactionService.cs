using Fintech.Api.Models;

namespace Fintech.Api.Services
{
    public interface ITransactionService
    {
        public Task<Transaction?> GetTransactionAsync(Guid id);
        public Task<IEnumerable<Transaction>> GetTransactionsAsync(Guid accountId);
        public Task<Transaction?> CreateTransactionAsync(Transaction transaction);
    }
}