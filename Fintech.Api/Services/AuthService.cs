using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fintech.Api.Data;
using Fintech.Api.DTOs;
using Fintech.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Fintech.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AuthService> _logger;
        private readonly HashSet<string> _blacklist;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public AuthService(AppDbContext dbContext, ILogger<AuthService> logger, HashSet<string> blacklist, IConfiguration configuration, ITokenService tokenService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _blacklist = blacklist;
            _configuration = configuration;
            _tokenService = tokenService;
        }
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation("User {username} trying to login...", request.Username);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

            if (user == null)
            {
                _logger.LogError("User {username} not found.", request.Username);
                return null;
            }

            _logger.LogInformation("User {username} logged in.", request.Username);

            return new LoginResponse { User = user, Token = await _tokenService.GenerateTokenAsync(user) };
        }

        public async Task<bool> LogoutAsync(LogoutRequest request)
        {
            await _tokenService.RevokeTokenAsync(request.Token);
            _logger.LogInformation("Token {token} revoked.", request.Token);
            return true;
        }
    }
}