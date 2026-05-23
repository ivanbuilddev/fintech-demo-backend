using Fintech.Api.Models;

namespace Fintech.Api.Services.Transactions
{
    public interface ITransactionStrategy
    {
        public TransactionType Type { get; }
        public Task<Transaction?> CreateTransactionAsync(Transaction transaction);
    }
}