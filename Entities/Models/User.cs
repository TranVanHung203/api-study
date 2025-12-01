using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;

        // Email xác nhận
        public bool EmailConfirmed { get; set; } = false;
    public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationExpiry { get; set; }

    // Is guest user (true = guest, false = registered)
    public bool IsGuest { get; set; } = false;

        // Guest user timeout - auto delete after this time
        public DateTime? ExpiresAt { get; set; }

        // Refresh Tokens
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
