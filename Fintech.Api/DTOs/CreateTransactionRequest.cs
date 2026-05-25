using Fintech.Api.Models;

namespace Fintech.Api.DTOs;

public class CreateTransactionRequest
{
    public Guid SourceAccountId { get; set; }
    public Guid DestinationAccountId { get; set; }
    public string Description { get; set; } = "New Transaction";
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; } = TransactionType.Transfer;
}