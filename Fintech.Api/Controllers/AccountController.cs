using Fintech.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Fintech.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Fintech.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [HttpGet("account/{id}")]
        public async Task<IActionResult> GetAccountById(Guid id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if(account == null)
            {
                return NotFound(new { message = "Account not found" });
            }
            return Ok(account);
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccountsByUserId(Guid userId)
        {
            var accounts = await _accountService.GetAccountsAsync(userId);
            if(accounts == null)
            {
                return NotFound(new { message = "Accounts not found" });
            }
            return Ok(accounts);
        }

        [HttpPost("accounts")]
        public async Task<IActionResult> CreateAccount([FromBody] Account account)
        {
            var accountCreated = await _accountService.CreateAccountAsync(account);
            if(accountCreated == null)
            {
                return NotFound(new { message = "Account could not be created" });
            }
            return Ok(accountCreated);
        }

        [HttpPut("accounts")]
        public async Task<IActionResult> UpdateAccount([FromBody] Account account)
        {
            var accountUpdated = await _accountService.UpdateAccountAsync(account);
            if(accountUpdated == null)
            {
                return NotFound(new { message = "Account could not be updated" });
            }
            return Ok(accountUpdated);
        }
    }
}