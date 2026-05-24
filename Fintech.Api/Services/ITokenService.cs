using Fintech.Api.Models;

namespace Fintech.Api.Services
{
    public interface ITokenService
    {
        public Task RevokeTokenAsync(string token);
        public Task<string> GenerateTokenAsync(User user);
        public Task CleanUpTokenAsync();
        public Task<bool> IsTokenRevoked(string token);
    }
}