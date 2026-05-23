using Fintech.Api.DTOs;
using Fintech.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpGet("transaction/{id}")]
        public async Task<IActionResult> GetTransactionById(Guid id)
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if(transaction == null)
            {
                return NotFound(new { message = "Transaction not found" });
            }
            return Ok(transaction);
        }

        [HttpGet("transactions/{id}")]
        public async Task<IActionResult> GetTransactionsByAccountId(Guid id)
        {
            var transaction = await _transactionService.GetTransactionsByAccountIdAsync(id);
            if(transaction == null)
            {
                return NotFound(new { message = "Transaction not found" });
            }
            return Ok(transaction);
        }

        [HttpPost("transactions")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransationRequest transaction)
        {
            var transactionCreated = await _transactionService.CreateTransactionAsync(transaction);
            if(transactionCreated == null)
            {
                return NotFound(new { message = "Transaction could not be created" });
            }
            return Ok(transactionCreated);
        }
    }
}