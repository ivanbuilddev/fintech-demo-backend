using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fintech.Api.Models;
public class Transaction
{
    [Key]
    public Guid Id { get; set; }
    public Guid? SourceAccountId { get; set; }
    public Guid? DestinationAccountId { get; set; }
    public string Description { get; set; } = "New Transaction";
    public string Category { get; set; } = "General";
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal Amount { get; set; } = 0;
    [Required]
    public TransactionType Type { get; set; } = TransactionType.Transfer;
    public DateTime CreatedAtUtc { get; set; } = DateTime.Now;
}

public enum TransactionType
{
    Transfer,
    Withdrawal,
    Deposit
}