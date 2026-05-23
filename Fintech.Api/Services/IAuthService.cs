using Fintech.Api.Models;

namespace Fintech.Api.Services
{
    public interface IAuthService
    {
        public Task<User?> LoginAsync(string username);
        public Task<bool> LogoutAsync(string token);
        public Task<bool> IsTokenRevokedAsync(string token);
    }
}