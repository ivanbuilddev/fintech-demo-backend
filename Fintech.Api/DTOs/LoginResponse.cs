using Fintech.Api.Models;

namespace Fintech.Api.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public User? User { get; set; }
    }
}