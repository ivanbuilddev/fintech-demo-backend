using Fintech.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Fintech.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Fintech.Api.DTOs;

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
            if (id == Guid.Empty)
                return BadRequest(new { message = "Invalid account ID" });

            var account = await _accountService.GetAccountByIdAsync(id);
            if(account == null)
            {
                return NotFound(new { message = "Account not found" });
            }
            return Ok(account);
        }

        [HttpGet("accounts/{id}")]
        public async Task<IActionResult> GetAccountsByUserId(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Invalid user ID" });

            var accounts = await _accountService.GetAccountsAsync(id);
            if(accounts == null)
            {
                return NotFound(new { message = "Accounts not found" });
            }
            return Ok(accounts);
        }

        [HttpPost("accounts")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            var accountCreated = await _accountService.CreateAccountAsync(request);
            if(accountCreated == null)
            {
                return NotFound(new { message = "Account could not be created" });
            }
            return Ok(accountCreated);
        }

        [HttpPut("accounts/{id}")]
        public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountRequest request)
        {
            var accountUpdated = await _accountService.UpdateAccountAsync(id, request);
            if(accountUpdated == null)
            {
                return NotFound(new { message = "Account could not be updated" });
            }
            return Ok(accountUpdated);
        }
    }
}