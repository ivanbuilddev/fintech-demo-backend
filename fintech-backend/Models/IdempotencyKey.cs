using System.ComponentModel.DataAnnotations;

namespace fintech_backend.Models
{
    public class IdempotencyKey
    {
        [Key]
        [MaxLength(450)]
        public string ResourceKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string RequestPath { get; set; } = string.Empty;

        [Required]
        public int ResponseStatusCode { get; set; }

        [Required]
        public string ResponseBody { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiresAtUtc { get; set; }
    }
}