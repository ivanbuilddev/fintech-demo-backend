using Fintech.Api.DTOs;

namespace Fintech.Api.Services
{
    public interface IAuthService
    {
        public Task<LoginResponse?> LoginAsync(string username);
        public Task<bool> LogoutAsync(string token);
        public Task<bool> IsTokenRevokedAsync(string token);
    }
}