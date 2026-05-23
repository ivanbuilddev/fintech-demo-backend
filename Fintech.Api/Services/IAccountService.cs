using Fintech.Api.DTOs;
using Fintech.Api.Models;

namespace Fintech.Api.Services
{
    public interface IAccountService
    {
        public Task<Account?> GetAccountByIdAsync(Guid id);
        public Task<IEnumerable<Account>> GetAccountsAsync(Guid userId);
        public Task<Account> CreateAccountAsync(CreateAccountRequest request);
        public Task<Account?> UpdateAccountAsync(Guid id, UpdateAccountRequest request);
    }
}