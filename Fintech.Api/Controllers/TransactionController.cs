using Fintech.Api.DTOs;
using Fintech.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Api.Controllers;
    
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionController : CustomControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IAccountService _accountService;
    private readonly ILogger<TransactionController> _logger;

    public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger, IAccountService accountService)
    {
        _transactionService = transactionService;
        _logger = logger;
        _accountService = accountService;
    }

    [HttpGet("transaction/{id}")]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        _logger.LogInformation("Transaction with id {id} requested.", id);
        var transaction = await _transactionService.GetTransactionByIdAsync(id);
        if(transaction == null)
        {
            _logger.LogError("Transaction with id {id} not found.", id);
            return NotFound(new { message = "Transaction not found" });
        }
        _logger.LogInformation("Transaction with id {id} found.", id);
        return Ok(transaction);
    }

    [HttpGet("transactions/{id}")]
    public async Task<IActionResult> GetTransactionsByAccountId(Guid id)
    {
        _logger.LogInformation("Transactions requested for account {id}.", id);
        var account = await _accountService.GetAccountByIdAsync(id);
        if(account == null)
        {
            _logger.LogError("Account with id {id} not found.", id);
            return NotFound(new { message = "Account not found" });
        }
        var transaction = await _transactionService.GetTransactionsByAccountIdAsync(id);
        _logger.LogInformation("Transactions found for account {id}.", id);
        return Ok(transaction);
    }

    [HttpPost("transactions")]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest transaction)
    {
        _logger.LogInformation("Transaction requested to be created.");
        var transactionCreated = await _transactionService.CreateTransactionAsync(transaction);
        if(transactionCreated == null)
        {
            _logger.LogError("Transaction could not be created.");
            return NotFound(new { message = "Transaction could not be created" });
        }
        _logger.LogInformation("Transaction created.");
        return Ok(transactionCreated);
    }
}