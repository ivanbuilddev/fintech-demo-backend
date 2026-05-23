using Microsoft.AspNetCore.Mvc;
using Fintech.Api.Services;
using Fintech.Api.Models;
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
        public async Task<IActionResult> Login([FromBody] string request)
        {
            var user = await _authService.LoginAsync(request);
            if(user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }
            return Ok(user);
        }
        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string token)
        {
            var result = await _authService.LogoutAsync(token);
            if(!result)
            {
                return Unauthorized(new { message = "Invalid token" });
            }
            return Ok(new { message = "Successfully logged out" });
        }
    }
}