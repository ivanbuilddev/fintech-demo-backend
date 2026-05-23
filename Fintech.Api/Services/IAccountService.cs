using Fintech.Api.Models;

namespace Fintech.Api.Services
{
    public interface IAccountService
    {
        public Task<Account?> GetAccountByIdAsync(Guid id);
        public Task<IEnumerable<Account>> GetAccountsAsync(Guid userId);
        public Task<Account> CreateAccountAsync(Account account);
        public Task<Account?> UpdateAccountAsync(Account account);
    }
}