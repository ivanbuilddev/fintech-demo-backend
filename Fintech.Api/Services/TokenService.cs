using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fintech.Api.Data;
using Fintech.Api.Models;
using Fintech.Api.Services.Auth;
using Microsoft.IdentityModel.Tokens;

namespace Fintech.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<TokenService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HashSet<RevokedToken> _blacklist;

        private readonly object _lock = new object();

        public TokenService(AppDbContext dbContext, ILogger<TokenService> logger, IConfiguration configuration, HashSet<RevokedToken> blacklist)
        {
            _dbContext = dbContext;
            _logger = logger;
            _configuration = configuration;
            _blacklist = blacklist;
        }

        public async Task RevokeTokenAsync(string token)
        {
            lock (_lock)
            {
                _blacklist.Add(new RevokedToken { Token = token });
            }
        }

        public async Task<string> GenerateTokenAsync(User user)
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

        public async Task CleanUpTokenAsync()
        {
            var now = DateTime.UtcNow;
        
            lock (_lock)
            {
                _blacklist.RemoveWhere(item => item.RevokedAt < now);
            }
        }

        public async Task<bool> IsTokenRevoked(string token)
        {
            var revoked = _blacklist.FirstOrDefault(item => item.Token == token);

            if (revoked == null)
            {
                return false;
            }

            return true;
        }
    }  
}