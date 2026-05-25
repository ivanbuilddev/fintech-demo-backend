using Fintech.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Fintech.Api.DTOs;

namespace Fintech.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AccountController : CustomControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAccountService accountService, ILogger<AccountController> logger, IUserService userService)
    {
        _accountService = accountService;
        _logger = logger;
        _userService = userService;
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

        var user = await _userService.GetUserByIdAsync(id);
        if(user == null) return NotFound(new { message = "User not found" });

        var accounts = await _accountService.GetAccountsAsync(id);
        return Ok(accounts);
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        var userId = GetCurrentUserGuid();
        var user = await _userService.GetUserByIdAsync(userId);
        if(user == null) return NotFound(new { message = "User not found" });

        var accountCreated = await _accountService.CreateAccountAsync(userId, request);
        return Ok(accountCreated);
    }

    [HttpPut("accounts/{id}")]
    public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountRequest request)
    {
        var user = await _userService.GetUserByIdAsync(GetCurrentUserGuid());
        if(user == null) return NotFound(new { message = "User not found" });

        var accountUpdated = await _accountService.UpdateAccountAsync(id, request);
        if(accountUpdated == null)
        {
            return NotFound(new { message = "Account not found" });
        }
        return Ok(accountUpdated);
    }
}
