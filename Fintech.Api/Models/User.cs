using System.ComponentModel.DataAnnotations;

namespace Fintech.Api.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        [Required]
        public UserRoles Role { get; set; } = UserRoles.User;
        public DateTime CreatedAtUtc { get; set; } = DateTime.Now;
    }
}

public enum UserRoles
{
    Admin,
    User
}