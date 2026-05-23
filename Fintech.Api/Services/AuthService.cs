using Fintech.Api.Data;
using Fintech.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Fintech.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AuthService> _logger;
        private readonly HashSet<string> _blacklist;

        public AuthService(AppDbContext dbContext, ILogger<AuthService> logger, HashSet<string> blacklist)
        {
            _dbContext = dbContext;
            _logger = logger;
            _blacklist = blacklist;
        }
        public async Task<User?> LoginAsync(string username)
        {
            _logger.LogInformation("User {username} trying to login...", username);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            if (user == null)
            {
                _logger.LogError("User {username} not found.", username);
                return null;
            }

            _logger.LogInformation("User {username} logged in.", username);

            return user;
        }

        public async Task<bool> LogoutAsync(string token)
        {
            lock (_blacklist)
            {
                _blacklist.Add(token);
            }
            _logger.LogInformation("Token {token} revoked.", token);
            return true;
        }

        public async Task<bool> IsTokenRevokedAsync(string token)
        {
            lock (_blacklist)
            {
                return _blacklist.Contains(token);
            }
        }
    }
}