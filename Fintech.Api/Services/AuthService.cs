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

        public AuthService(AppDbContext dbContext, ILogger<AuthService> logger, HashSet<string> blacklist, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _blacklist = blacklist;
            _configuration = configuration;
        }
        public async Task<LoginResponse?> LoginAsync(string username)
        {
            _logger.LogInformation("User {username} trying to login...", username);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            if (user == null)
            {
                _logger.LogError("User {username} not found.", username);
                return null;
            }

            _logger.LogInformation("User {username} logged in.", username);

            return new LoginResponse { User = user, Token = GenerateToken(user) };
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

        private string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = creds,
                Audience = _configuration["JwtSettings:Audience"],
                Issuer = _configuration["JwtSettings:Issuer"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);  
        }
    }
}