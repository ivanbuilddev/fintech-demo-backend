using Microsoft.AspNetCore.Mvc;
using Fintech.Api.Services;
using Fintech.Api.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Fintech.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _authService.LoginAsync(request);
            if(user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }
            return Ok(user);
        }
        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer ", "");
            var expireClaim = User.FindFirst("exp")?.Value;
            var expDate = DateTime.Now;
            if (long.TryParse(expireClaim, out long expUnix))
            {
                expDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            }
            LogoutRequest request = new LogoutRequest { Token = token, ExpireDate = expDate };
            var result = await _authService.LogoutAsync(request);
            if(!result)
            {
                return Unauthorized(new { message = "Invalid token" });
            }
            return Ok(new { message = "Successfully logged out" });
        }
    }
}