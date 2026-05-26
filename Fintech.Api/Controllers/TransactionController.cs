using System.Text.Json;
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
    private readonly IIdempotencyService _idempotencyService;

    public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger, IAccountService accountService, IIdempotencyService idempotencyService)
    {
        _transactionService = transactionService;
        _logger = logger;
        _accountService = accountService;
        _idempotencyService = idempotencyService;
    }

    [HttpGet("transaction/{id}")]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        _logger.LogInformation("Transaction with id {id} requested.", id);
        var transaction = await _transactionService.GetTransactionByIdAsync(id, GetCurrentUserId());
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
        var account = await _accountService.GetAccountByIdAsync(id, GetCurrentUserId());
        if(account == null)
        {
            _logger.LogError("Account with id {id} not found.", id);
            return NotFound(new { message = "Account not found" });
        }
        var transaction = await _transactionService.GetTransactionsByAccountIdAsync(id, GetCurrentUserId());
        _logger.LogInformation("Transactions found for account {id}.", id);
        return Ok(transaction);
    }

    [HttpPost("transactions")]
    public async Task<IActionResult> CreateTransaction([FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, [FromBody] CreateTransactionRequest transaction)
    {
        _logger.LogInformation("Transaction requested to be created.");

        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest("Idempotency-Key header is required.");

        var idempotencyResponseDuplicate = await _idempotencyService.IsDuplicateAsync(idempotencyKey, "POST api/transactions");
        if(idempotencyResponseDuplicate != null) return StatusCode(idempotencyResponseDuplicate.StatusCode, idempotencyResponseDuplicate.ReponseBody);

        var transactionCreated = await _transactionService.CreateTransactionAsync(transaction, GetCurrentUserId());
        if(transactionCreated == null)
        {
            _logger.LogError("Transaction could not be created.");
            await _idempotencyService.CreateAsync(idempotencyKey, "POST api/transactions", StatusCodes.Status404NotFound, "Transaction could not be created");
            return NotFound(new { message = "Transaction could not be created" });
        }
        _logger.LogInformation("Transaction created.");
        var responseJson = JsonSerializer.Serialize(transactionCreated);
        await _idempotencyService.CreateAsync(idempotencyKey, "POST api/transactions", StatusCodes.Status200OK, responseJson);
        return Ok(transactionCreated);
    }

    [HttpDelete("transactions/{id}")]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        _logger.LogInformation("Transaction with id {id} requested to be deleted.", id);
        var transaction = await _transactionService.GetTransactionByIdAsync(id, GetCurrentUserId());
        if(transaction == null)
        {
            _logger.LogError("Transaction with id {id} not found.", id);
            return NotFound(new { message = "Transaction not found" });
        }

        var transactionDeleted = await _transactionService.DeleteTransactionAsync(id, GetCurrentUserId());
        if(!transactionDeleted)
        {
            _logger.LogError("Transaction with id {id} could not be deleted.", id);
            return NotFound(new { message = "Transaction could not be deleted" });
        }
        _logger.LogInformation("Transaction with id {id} deleted.", id);
        return Ok(("Transaction {id} deleted", id));
    }
}