using Fintech.Api.DTOs;
using Fintech.Api.Models;

namespace Fintech.Api.Services;
public interface IAccountService
{
    public Task<Account?> GetAccountByIdAsync(Guid id, Guid currentUserId);
    public Task<Account?> GetAccountByIdAsync(Guid id);
    public Task<IEnumerable<Account>> GetAccountsAsync(Guid userId, Guid currentUserId);
    public Task<Account> CreateAccountAsync(Guid userId, CreateAccountRequest request);
    public Task<Account?> UpdateAccountAsync(Guid id, UpdateAccountRequest request, Guid currentUserId);
    public Task<bool> DeleteAccountAsync(Guid id, Guid currentUserId);
    public Task<bool> ForceDeleteAccountAsync(Guid id);
    public Task<Account?> RecalculateAccountBalanceAsync(Guid id);
}