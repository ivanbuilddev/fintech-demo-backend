using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fintech_backend.Models
{
    public class Account
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Balance { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "EUR";

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}